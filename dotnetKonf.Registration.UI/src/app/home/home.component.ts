import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EntryResponseModel } from 'src/models/EntryResponseModel';
import { ApiResponse } from 'src/models/ApiResponse';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  apiUri: string = "/api/register";
  response: ApiResponse<object> = new ApiResponse<object>();
  entryId: string;
  entryForm: FormGroup;
  verifyForm: FormGroup;
  isSending: boolean = false;
  isResendingCode: boolean = false;
  constructor(private http: HttpClient, private formBuilder: FormBuilder) {
    this.initEntryForm();
    this.initVerifyForm();
  }

  initEntryForm() {
    this.entryForm = this.formBuilder.group({
      name: ['', Validators.required],
      surname: ['', Validators.required],
      company: ['', Validators.required],
      title: ['', Validators.required],
      mailAddress: ['', Validators.compose([Validators.required, Validators.email])],
      phoneNumber: ['+90', Validators.compose([Validators.required, Validators.minLength(13), Validators.maxLength(16), Validators.pattern('[+][0-9]*')])],
      licenceTerms: [false, Validators.requiredTrue]
    });
  }

  onEntryFormSubmited() {
    if (this.entryForm.valid) {
      this.isSending = true;
      this.http.post<ApiResponse<EntryResponseModel>>(this.apiUri, this.entryForm.value).subscribe(data => {
        this.isSending = false;
        this.response.isSucceeded = data.isSucceeded;
        this.response.message = data.message;
        if (data.isSucceeded)
          this.entryId = data.value.id;
      })
    }
  }

  initVerifyForm() {
    this.verifyForm = this.formBuilder.group({
      verifyCode: ['', Validators.compose([Validators.required, Validators.minLength(6), Validators.maxLength(6)])]
    })
  }
  
  onVerifyFormSubmitted() {
    if(this.verifyForm.valid) {
      var request = this.verifyForm.value;
      request.action = "verify";
      this.isSending = true;
      this.http.put<ApiResponse<object>>(this.apiUri + '/' + this.entryId, request).subscribe(data => {
        this.isSending = false;
        this.response.isSucceeded = data.isSucceeded;
        this.response.message = data.message;
      });
    }
  }

  onResetCode(){
    if(this.entryId){
      var request = { action: "reset" };
      this.isResendingCode = true;
      this.http.put<ApiResponse<object>>(this.apiUri + '/' + this.entryId, request).subscribe(data => {
        this.isResendingCode = false;
        this.response.isSucceeded = data.isSucceeded;
        this.response.message = data.message;
      });
    }
  }

  ngOnInit() {
  }

}
