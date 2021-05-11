import { Component, OnInit } from '@angular/core';
import { Md5} from 'ts-md5/dist/md5';
import { PreviewService } from 'src/app/services/preview.service';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-preview',
  templateUrl: './preview.component.html',
  styleUrls: ['./preview.component.css']
})
export class PreviewComponent implements OnInit {

  constructor(
    private previewService: PreviewService,
    private membersService: MembersService
    ) { }

  ngOnInit(): void {
  }

  public previewCode!: string;
  public codeResult!: string;

  onSubmit(): void {

    var codeHash = Md5.hashStr(this.previewCode.toUpperCase()).toString();

    if (codeHash == "c8a5a445cd4cdaf92c29939458fd7c22")
    {
      this.previewService.previewCodeValid = true;
      this.membersService.IsAdmin = true;
    }
    else if (codeHash == "0a4f0d017f0b4c75c14f9012661f68cc")
    {
      this.previewService.previewCodeValid = true;
      this.membersService.IsAdmin = false;
    }
    else
    {
      this.codeResult = "Sorry, that code is invalid!";
    }
  }

}
