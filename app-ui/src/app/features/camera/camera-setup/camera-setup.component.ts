import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { NgForOf } from '@angular/common';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '../../../components/button/button.component';

@Component({
  selector: 'app-camera-setup',
  imports: [NgForOf, FormsModule, ReactiveFormsModule, ButtonComponent],
  templateUrl: './camera-setup.component.html',
  styleUrl: './camera-setup.component.scss',
})
export class CameraSetupComponent implements OnInit {
  @Output() cameraSelected = new EventEmitter<string>();

  private logText = '';

  protected cameraDevices: MediaDeviceInfo[] = [];
  protected cameraSelect = new FormControl('');

  async ngOnInit() {
    await this.requestCameraPermission();
    await this.initCameraOptions();
  }

  async requestCameraPermission() {
    const constraints = { video: true, audio: false };
    const stream = await navigator.mediaDevices.getUserMedia(constraints);
    const tracks = stream.getTracks();
    for (const track of tracks) {
      track.stop();
    }
  }

  async initCameraOptions() {
    this.log('gettings cameras...');

    const devices = await navigator.mediaDevices.enumerateDevices();
    const videoDevices = devices.filter(
      (device) => device.kind === 'videoinput',
    );
    this.log('got cameras', videoDevices.length);

    this.cameraDevices = videoDevices;
  }

  selectCamera() {
    if (this.cameraSelect.value) {
      this.cameraSelected.emit(this.cameraSelect.value);
    }
  }

  // eslint-disable-next-line @typescript-eslint/ban-ts-comment
  // @ts-expect-error
  private log(...args) {
    console.log(...args);
    let text = '';
    for (const arg of args) {
      text += arg + ' ';
    }
    this.logText += text + '<br/>';
  }
}
