import { AfterViewInit, Component, ElementRef, Inject, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { MAT_DATE_FORMATS } from '@angular/material/core';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';
import { RefData } from 'src/app/models/refData';
import { RefDataService } from 'src/app/services/ref-data.service';
import { GuestTicket } from 'src/app/models/guest-ticket';
import { ScreenService } from 'src/app/services/screen.service';
import { ThrowStmt } from '@angular/compiler';


export interface DialogData {
  dbKey: string;
}

export const MY_FORMATS = {
  parse: {
    dateInput: 'LL',
  },
  display: {
    dateInput: 'DD MMM YY',
    monthYearLabel: 'YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'YYYY',
  },
};


// Sizes in mm
const MARGIN = 10;         
const TICKET_WIDTH  = 173; 
const TICKET_HEIGHT = 82;  
const VERTICAL_DIVIDER = 60;
const PRICE_DIVIDER = 19;
const TICKET_NUMBER_DIVIDER = 55;


@Component({
  selector: 'app-create-guest-ticket',
  templateUrl: './create-guest-ticket.component.html',
  styleUrls: ['./create-guest-ticket.component.css']
})

export class CreateGuestTicketComponent implements OnInit, AfterViewInit {

  private SCALE = 3.5;
  private CANVAS_WIDTH  = (TICKET_WIDTH * this.SCALE) + (MARGIN * this.SCALE * 2);
  private CANVAS_HEIGHT = (TICKET_HEIGHT * this.SCALE) + (MARGIN * this.SCALE * 2);

  public title: string;
  public selectedMember!: Member;
  public members!: Member[];
  public refData!: RefData;

  public isLoading: boolean = false;

  public guestTicket!: GuestTicket;

  @ViewChild('canvasEl', { static: false }) canvas!: ElementRef<HTMLCanvasElement>; 

  private context!: CanvasRenderingContext2D;

  constructor(public dialogRef: MatDialogRef<CreateGuestTicketComponent>,
    private membersService: MembersService,
    private refDataService: RefDataService,
    public screenService: ScreenService,

    @Inject(MAT_DIALOG_DATA) public data: DialogData) {
      if (data.dbKey != "") {
        this.title = "Edit Guest Ticket";
      } else {
        this.title = "Issue Guest Ticket";
      }

      screenService.OrientationChange.on(() => {
        this.setCanvasSCALE(screenService.IsHandsetPortrait);
      });
  }
    

  public cancel(): void {
    this.dialogRef.close();
  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();

    this.guestTicket = new GuestTicket();

  }

  ngAfterViewInit() {
    //alert("after view init");
    //this.canvas.nativeElement.width = this.CANVAS_WIDTH;
    //this.canvas.nativeElement.height = this.CANVAS_HEIGHT;
 
    //this.canvas.nativeElement.style.backgroundColor = "Yellow";

    this.context = <CanvasRenderingContext2D>(
      this.canvas.nativeElement as HTMLCanvasElement
    ).getContext("2d");

    this.setCanvasSCALE(this.screenService.IsHandsetPortrait);
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.getMembers();
    });
  }

  public getMembers() {
    this.membersService.readAllMembers()
    .subscribe(data => {
      this.members = data as Member[];
      this.members = this.members
      .sort((a, b) => {
        return b.surname < a.surname && 1 || -1;
      })
      this.isLoading = false;
    });
  }


  public onChangeEvent(event: any) {
    this.previewTicket();
  }

  public previewTicket() {

    this.guestTicket.cost = 5;
    //this.guestTicket.ticketNumber ;
    this.guestTicket.issuedBy = this.membersService.CurrentMember.name;
    this.guestTicket.issuedOn = new Date();
    this.guestTicket.membersName = this.selectedMember.name;
    this.guestTicket.emailTo = this.selectedMember.email;

    if (this.guestTicket.ticketValidOn == null || this.selectedMember == null || this.guestTicket.guestsName == null || this.guestTicket.guestsName.trim() == "") {
      return;
    }

    this.drawTicket(this.context);
  }

  drawTicket(ctx: CanvasRenderingContext2D): void {

    ctx.beginPath();

    ctx.clearRect(0, 0, this.CANVAS_WIDTH, this.CANVAS_HEIGHT);

    ctx.fillStyle = "#FFFFFF";
    ctx.fillRect(0, 0, this.CANVAS_WIDTH, this.CANVAS_HEIGHT);

    ctx.fillStyle = "#000000";
    ctx.rect(MARGIN * this.SCALE, MARGIN * this.SCALE, TICKET_WIDTH * this.SCALE, TICKET_HEIGHT * this.SCALE);

    ctx.moveTo((MARGIN + VERTICAL_DIVIDER) * this.SCALE, MARGIN * this.SCALE);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * this.SCALE, (MARGIN + TICKET_HEIGHT) * this.SCALE);

    ctx.moveTo(MARGIN * this.SCALE, (MARGIN + PRICE_DIVIDER) * this.SCALE);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * this.SCALE,  (MARGIN + PRICE_DIVIDER) * this.SCALE);

    ctx.moveTo(MARGIN * this.SCALE, (MARGIN + TICKET_NUMBER_DIVIDER) * this.SCALE);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * this.SCALE,  (MARGIN + TICKET_NUMBER_DIVIDER) * this.SCALE);

    // e.g. context.font="italic small-caps bold 12px arial";
    ctx.font=`normal normal normal ${16 / 3.5 * this.SCALE}px arial`;

    ctx.textAlign = "center";
    ctx.fillText("£" + (Math.round(this.guestTicket.cost * 100) / 100).toFixed(2), (MARGIN + (VERTICAL_DIVIDER / 2)) * this.SCALE, (MARGIN + 11) * this.SCALE);

    ctx.fillText("TICKET NO.", (MARGIN + (VERTICAL_DIVIDER / 2)) * this.SCALE, (MARGIN + 26) * this.SCALE);
    ctx.fillText((this.guestTicket.ticketNumber == null ? "Generated when sent" : ("On line/" + this.addLeadingZeros(this.guestTicket.ticketNumber, 4))), (MARGIN + (VERTICAL_DIVIDER / 2)) * this.SCALE, (MARGIN + 33) * this.SCALE);

    ctx.font=`normal normal normal ${12 / 3.5 * this.SCALE}px arial`;

    ctx.textAlign = "left";
    ctx.fillText("Issued by: " + this.guestTicket.issuedBy, (MARGIN + 9) * this.SCALE, (MARGIN + 42) * this.SCALE);
    ctx.fillText("Issued on: " + this.guestTicket.issuedOn.toDateString(), (MARGIN + 9) * this.SCALE, (MARGIN + 48) * this.SCALE);

    ctx.textAlign = "center";
    ctx.fillText("Ticket Covers:", (MARGIN + (VERTICAL_DIVIDER / 2)) * this.SCALE, (MARGIN + 60) * this.SCALE);

    ctx.textAlign = "left";
    ctx.fillText("Date: " + this.guestTicket.ticketValidOn!.toDateString() , (MARGIN + 9) * this.SCALE, (MARGIN + 70) * this.SCALE);

    ctx.textAlign = "center";
    ctx.font=`normal normal bold ${14 / 3.5 * this.SCALE}px arial`;
    ctx.fillText("BOROUGHBRIDGE & DISTRICT ANGLING CLUB", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * this.SCALE, (MARGIN + 10) * this.SCALE);

    ctx.font=`normal normal bold ${18 / 3.5 * this.SCALE}px arial`;
    ctx.fillText("MEMBERS GUEST TICKET", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * this.SCALE, (MARGIN + 25) * this.SCALE);

    ctx.font=`normal normal normal ${12 / 3.5 * this.SCALE}px arial`;
    ctx.fillText("for the", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * this.SCALE, (MARGIN + 34) * this.SCALE);
    ctx.font=`normal normal normal ${14 / 3.5 * this.SCALE}px arial`;
    ctx.fillText("CLUB WATERS: RIVER URE & ROECLIFFE POND", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * this.SCALE, (MARGIN + 40) * this.SCALE);

    ctx.textAlign = "left";
    ctx.fillText("MEMBERS NAME: " + this.guestTicket.membersName, (MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE, (MARGIN + 50) * this.SCALE);
    ctx.fillText("GUESTS NAME: " + this.guestTicket.guestsName, (MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE, (MARGIN + 58) * this.SCALE);

    ctx.textAlign = "left";
    ctx.font=`normal normal normal ${12 / 3.5 * this.SCALE}px arial`;
    ctx.fillText("NO TICKETS AVAILABLE ON ANY SUNDAY MATCH VENUES", (MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE, (MARGIN + 70) * this.SCALE);
    ctx.font=`normal normal normal ${10 / 3.5 * this.SCALE}px arial`;
    var noticeText = "Please read the rules and bait bans on the notice board ";
    var noticeTextWidth = ctx.measureText(noticeText).width;
    ctx.fillText(noticeText, (MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE, (MARGIN + 74) * this.SCALE);

    ctx.fillStyle = "#FF0000";
    ctx.font=`normal normal bold ${10 / 3.5 * this.SCALE}px arial`;
    ctx.fillText("before", ((MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE) + noticeTextWidth, (MARGIN + 74) * this.SCALE);
    var before = "before";
    var beforeWidth = ctx.measureText(before).width;
    ctx.moveTo(((MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE) + noticeTextWidth,  ((MARGIN + 74) * this.SCALE) + 2);
    ctx.lineTo(((MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE) + noticeTextWidth + beforeWidth,  ((MARGIN + 74) * this.SCALE) + 2);

    ctx.fillStyle = "#000000";
    ctx.font=`normal normal normal ${10 / 3.5 * this.SCALE}px arial`;
    var noticeTextBefore = noticeText + "before ";
    var noticeTextBeforeWidth = ctx.measureText(noticeTextBefore).width + 2;
    ctx.fillText("fishing.", ((MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE) + noticeTextBeforeWidth, (MARGIN + 74) * this.SCALE);

    //ctx.fillText("Please read the rules and bait bans on the notice board before fishing.", (MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE, (MARGIN + 74) * this.SCALE);
    ctx.fillText("Members must fish with their guest and be responsible for them.", (MARGIN + VERTICAL_DIVIDER + 9) * this.SCALE, (MARGIN + 78) * this.SCALE);

    ctx.stroke();
    
    //var dataUrl = this.canvas.nativeElement.toDataURL('image/jpeg', 1.0);
    var dataUrl = this.canvas.nativeElement.toDataURL();
    console.log(dataUrl);
  }

  addLeadingZeros(num: number, totalLength: number): string {
    return String(num).padStart(totalLength, '0');
  }

  private setCanvasSCALE(isHandsetPortait: boolean) {
    if (isHandsetPortait) {
      this.SCALE = 1.2;
    } else {
      this.SCALE = 3.5;
    }

    this.CANVAS_WIDTH  = (TICKET_WIDTH * this.SCALE) + (MARGIN * this.SCALE * 2);
    this.CANVAS_HEIGHT = (TICKET_HEIGHT * this.SCALE) + (MARGIN * this.SCALE * 2);
  
    this.canvas.nativeElement.width = this.CANVAS_WIDTH;
    this.canvas.nativeElement.height = this.CANVAS_HEIGHT;

    //alert("setting scale to: " + this.SCALE);
    this.previewTicket();
  }
}


