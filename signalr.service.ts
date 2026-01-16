import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState } from '@microsoft/signalr';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { BehaviorSubject, from, Observable, Subject } from 'rxjs';
import { APP_CONFIG } from 'main'
import { ConfirmOfflineModeRequestNotification } from './model/confirm-offline-mode-request/ConfirmOfflineModeRequestNotification';
import { IndividualProgramRequestNotification } from './model/models';
import { OcrResult } from './model/ocr/OcrResult';
import { CommonService } from 'app/shared/common.service';
import { LogPatronSearchPhoneRequestNotification } from './model/log-patron-search-phone-request/LogPatronSearchPhoneRequestNotification';

export interface Phone {
  phoneId: number;
  phoneNumber: string;
}

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  ocrResultGroup = 'OcrResultGroup';

  private _offlineModeNotification: BehaviorSubject<ConfirmOfflineModeRequestNotification> = new BehaviorSubject<ConfirmOfflineModeRequestNotification>(null);

  private _testNotification: Subject<Phone> = new Subject<Phone>();

  private _cbppSendRequest: Subject<IndividualProgramRequestNotification> = new Subject<IndividualProgramRequestNotification>();

  private _ocrResult: Subject<OcrResult> = new Subject<OcrResult>();

  private _hubConnection: signalR.HubConnection;

  /**
   *
   */
  constructor(private _oidcSecurityService: OidcSecurityService, private _commonService: CommonService) {
  }

  get offlineModeNotification$(): Observable<ConfirmOfflineModeRequestNotification> {
    return this._offlineModeNotification.asObservable();
  }

  get cbppSendRequest$(): Observable<IndividualProgramRequestNotification> {
    return this._cbppSendRequest.asObservable();
  }

  get ocrResult$(): Observable<OcrResult> {
    return this._ocrResult.asObservable();
  }

  get testNotification$(): Observable<Phone> {
    return this._testNotification.asObservable();
  }

  public startConnection = () => {

    this._hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(APP_CONFIG.signalrPath, {
        accessTokenFactory: () => this._oidcSecurityService.getAccessToken()
      })
      .configureLogging(signalR.LogLevel.Error)
      .withAutomaticReconnect()
      .build();

    this._hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err));

    this._hubConnection.onreconnecting(error => {

    });
  }

  public connected(): boolean {
    if (this._hubConnection == undefined) return false;

    return this._hubConnection.state === HubConnectionState.Connected;
  }

  public addCheckOfflineModeNotificationListener(): void {
    this._hubConnection.on('CheckOfflineModeNotification', (data) => {
      this._offlineModeNotification.next(data as ConfirmOfflineModeRequestNotification);
      console.log(data);
    });
  }

  public addCbppSendRequestListener = () => {
    this._hubConnection.on('CbppSendRequestNotification', (data) => {
      this._cbppSendRequest.next(data as IndividualProgramRequestNotification);
    });
  }

  public addOcrResultListener = () => {
    from(this._hubConnection.invoke('JoinGroup', this.ocrResultGroup))
      .subscribe(r => {
        this._hubConnection.on('OcrResultNotification', (data) => {
          console.log("receive ocr result");
          let ocrResult = data as OcrResult;


          // REMARK: only process data that match current IP
          if (ocrResult.ocrMetaData?.localIp === this._commonService.ip.value) {
            this._ocrResult.next(data as OcrResult);
          }
        });
      });
  }

  public removeOcrResultListener = () => {
    from(this._hubConnection.invoke('LeaveGroup', this.ocrResultGroup))
      .subscribe(r => this._hubConnection.off('OcrResultNotification'));
  }

  public disconnectHub(): void {
    this._hubConnection.stop();
  }

  public broadcastCbppSendRequest(): void {
    // this._hubConnection.invoke('Testing', "testing message")
    //   .catch(err => console.error(err));

    this._hubConnection.stream("Counter", 10, 500)
      .subscribe({
        next: (item: Phone) => {
          // var li = document.createElement("li");
          // li.textContent = item;
          // document.getElementById("messagesList").appendChild(li);
          // console.log(item);

          this._testNotification.next(item);
        },
        complete: () => {
          // var li = document.createElement("li");
          // li.textContent = "Stream completed";
          // document.getElementById("messagesList").appendChild(li);
          console.log("stream completed");
        },
        error: (err) => {
          var li = document.createElement("li");
          li.textContent = err;
          document.getElementById("messagesList").appendChild(li);
        },
      });
  }

  public logPatronSearchPhoneAction = (request : LogPatronSearchPhoneRequestNotification) => {
    this._hubConnection.invoke('LogPatronSearchPhoneAction', request);
  }

  public logPatronAction = (request : LogPatronSearchPhoneRequestNotification) => {
    this._hubConnection.invoke('LogPatronAction', request);
  }
}
