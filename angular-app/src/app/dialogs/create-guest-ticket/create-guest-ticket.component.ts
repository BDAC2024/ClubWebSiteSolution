import { AfterViewInit, Component, ElementRef, Inject, NgZone, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { MAT_DATE_FORMATS } from '@angular/material/core';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';
import { RefData } from 'src/app/models/refData';
import { RefDataService } from 'src/app/services/ref-data.service';
import { GuestTicket } from 'src/app/models/guest-ticket';
import { ScreenService } from 'src/app/services/screen.service';
import { ThrowStmt } from '@angular/compiler';
import { GuestTicketService } from 'src/app/services/guest-ticket.service';
import { Observable } from 'rxjs';
import { map, startWith } from 'rxjs/operators';
import { ErrorComponent } from '../error/error.component';
import { ViewportRuler } from '@angular/cdk/scrolling';


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
const MARGIN = 1;         
const TICKET_WIDTH  = 173; 
const TICKET_HEIGHT = 82;  
const VERTICAL_DIVIDER = 60;
const PRICE_DIVIDER = 19;
const TICKET_NUMBER_DIVIDER = 55;

const DIALOG_USED_HEIGHT = 392; // pixels
const IDEAL_DIALOG_WIDTH = 672;
const IDEAL_DIALOG_HEIGHT = 700;

@Component({
  selector: 'app-create-guest-ticket',
  templateUrl: './create-guest-ticket.component.html',
  styleUrls: ['./create-guest-ticket.component.css']
})

export class CreateGuestTicketComponent implements OnInit, OnDestroy, AfterViewInit {
  myControl: FormControl = new FormControl();
  emailControl: FormControl = new FormControl();
  
  private SCALE = 3.5;
  private CANVAS_WIDTH  = (TICKET_WIDTH * this.SCALE) + (MARGIN * this.SCALE * 2);
  private CANVAS_HEIGHT = (TICKET_HEIGHT * this.SCALE) + (MARGIN * this.SCALE * 2);

  private screenWidth: number = 0;
  private screenHeight: number = 0;
  
  private dialogWidth: number = 0; // pixels
  private dialogHeight: number = 0; // pixels

  private ticketWidth: number = 0; // pixels
  private ticketHeight: number = 0; // pixels

  public title: string;
  public selectedMember!: Member;
  public members!: Member[];
  public refData!: RefData;
  public emailTo: string = "";
  public isSaving: boolean = false;

  public isLoading: boolean = false;
  filteredOptions!: Observable<Member[]>;
  editMode: boolean = false;

  //public guestTicket!: GuestTicket;

  @ViewChild('canvasEl', { static: false }) canvas!: ElementRef<HTMLCanvasElement>; 

  private context!: CanvasRenderingContext2D;

  private readonly viewportChange = this.viewportRuler
    .change(200)
    .subscribe(() => this.ngZone.run(() => this.setSize()));

  constructor(public dialogRef: MatDialogRef<CreateGuestTicketComponent>,
    private membersService: MembersService,
    private refDataService: RefDataService,
    public screenService: ScreenService,
    private guestTicketService: GuestTicketService,
    private dialog: MatDialog,
    private readonly viewportRuler: ViewportRuler,
    private readonly ngZone: NgZone,
    
    @Inject(MAT_DIALOG_DATA) public guestTicket: GuestTicket) {
      if (guestTicket.dbKey != "") {
        this.title = "Edit Guest Ticket";
        this.editMode = true;
      } else {
        this.title = "Issue Guest Ticket";
      }

      console.log(`Width: ${screenService.Width}`);
      console.log(`IsPortrait: ${screenService.IsPortrait}`);
      
      // screenService.OrientationChange.on(() => {
      //   //this.setCanvasSCALE(screenService.IsPortrait);
      // });

  }
    

  // Never forget to unsubscribe!
  ngOnDestroy() {
    this.viewportChange.unsubscribe();
  }

  private setSize() {
    const { width, height } = this.viewportRuler.getViewportSize();
    this.screenWidth = width;
    this.screenHeight = height;

    this.dialogWidth = width * 0.98;
    if (this.dialogWidth > IDEAL_DIALOG_WIDTH) {
      this.dialogWidth = IDEAL_DIALOG_WIDTH;
    }

    this.dialogHeight = height * 0.98;
    if (this.dialogHeight > IDEAL_DIALOG_HEIGHT) {
      this.dialogHeight = IDEAL_DIALOG_HEIGHT;
    }

    this.dialogRef.updateSize(`${this.dialogWidth}px`, `${this.dialogHeight}px`);
    //this.dialogRef.updateSize(`50%`, `50%`);

    this.ticketWidth = this.dialogWidth - 4;
    this.ticketHeight = this.dialogHeight - DIALOG_USED_HEIGHT;

    this.setCanvasSCALE();
  }

  public cancel(): void {
    if (!this.editMode && this.guestTicket.dbKey != null && this.guestTicket.dbKey.trim() != "") {
      this.guestTicketService.deleteGuestTicket(this.guestTicket.dbKey)
        .subscribe(data => {
          this.dialogRef.close();
        });
    } else {
      this.dialogRef.close();
    }
  }

  public send(): void {

    this.isSaving = true;

    this.guestTicketService.addOrUpdateGuestTicket(this.guestTicket)
    .subscribe(data => {
      this.guestTicketService.issueGuestTicket(this.guestTicket)
      .subscribe(data => {
        this.isSaving = false;

        const popupRef = this.dialog.open(ErrorComponent, {width: "300px", maxHeight: "100vh", data: {title: "Success", body: "Ticket has been emailed to " + this.guestTicket.emailTo}});
    
        popupRef.afterClosed().subscribe(result => {
          this.dialogRef.close(this.guestTicket);
        });
  
      }, err=> {
        this.isSaving = false;

        const popupRef = this.dialog.open(ErrorComponent, {width: "300px", maxHeight: "100vh", data: {title: "Failed", body: "Could not send ticket to " + this.guestTicket.emailTo + err}});
      });
    });

  }

  ngOnInit(): void {
    this.isLoading = true;
    this.getRefData();

    //this.guestTicket = new GuestTicket();
    this.filteredOptions = this.myControl.valueChanges
    .pipe(
    startWith(''),
    map(val => this.filter(val))
    );
  }

  filter(val: string): Member[] {
    return this.members.filter(option =>
      option.name.toLowerCase().includes(val.toLowerCase()));
  }

  updateSelectedMember(selectedMember: Member) {
    console.log("updateSelectedMember: Setting selectedMember = " + selectedMember.name);
    this.selectedMember = selectedMember;
    this.emailTo = selectedMember.email;
    this.emailControl.setValue(this.emailTo);
    console.log("updateSelectedMember: Setting emailTo = " + this.emailTo);
    this.onChangeEvent(selectedMember);
  }

  ngAfterViewInit() {
    //alert("after view init");
    //this.canvas.nativeElement.width = this.CANVAS_WIDTH;
    //this.canvas.nativeElement.height = this.CANVAS_HEIGHT;
 
    //this.canvas.nativeElement.style.backgroundColor = "Yellow";

    this.context = <CanvasRenderingContext2D>(
      this.canvas.nativeElement as HTMLCanvasElement
    ).getContext("2d");

    this.setSize();
  }

  public getRefData() {
    this.refDataService.getRefData()
    .subscribe(data => {
      this.refData = data;
      this.getMembers();
    });
  }

  public getMembers() {
    this.membersService.readMembers(this.guestTicket.season)
    .subscribe(data => {
      this.members = data as Member[];
      this.members = this.members
      .sort((a, b) => {
        return b.surname < a.surname && 1 || -1;
      })
      if (this.editMode) {
        this.emailTo = this.guestTicket.emailTo;
        this.selectedMember = this.members.filter(option =>
          option.membershipNumber == this.guestTicket.membershipNumber)[0];
          console.log("getMembers: Setting selectedMember to: " + this.selectedMember.name);
          this.myControl.setValue(this.selectedMember.name);
        this.onChangeEvent(this.selectedMember);
        this.previewTicket();
      }

      this.isLoading = false;
    });
  }

  public onEmailChangeEvent(event: any) {
    this.emailTo = this.emailControl.value;
  }


  public onChangeEvent(event: any) {

    if (!this.isLoading) {
      console.log("onChangeEvent: Previewing...");

      this.previewTicket();

      if (this.formComplete() && !this.editMode) {
        console.log("onChangeEvent: formComplete so updating...");

        this.guestTicketService.addOrUpdateGuestTicket(this.guestTicket)
          .subscribe(data => {
            this.guestTicket = data;
            console.log("onChangeEvent: Previewing again after updating...");
            this.previewTicket();
          });
      }
    }
  }

  public previewTicket() {

    //this.guestTicket.ticketNumber ;
    this.guestTicket.issuedBy = this.membersService.CurrentMember.name;
    this.guestTicket.issuedByMembershipNumber = this.membersService.CurrentMember.membershipNumber;
    this.guestTicket.issuedOn = new Date();
    //if (this.selectedMember != null) {
      this.guestTicket.membersName = this.selectedMember?.name;
      this.guestTicket.membershipNumber = this.selectedMember?.membershipNumber;
    //}
    this.guestTicket.emailTo = this.emailTo;

    this.drawTicket(this.context);
  }

  formComplete(): boolean {
    return !this.emailControl.invalid && 
          this.guestTicket.ticketValidOn != null &&
          this.selectedMember != null && 
          this.guestTicket.guestsName != null && 
          this.guestTicket.guestsName.trim() != "" &&
          this.guestTicket.emailTo != null &&
          this.guestTicket.emailTo.trim() != "";
  }

  drawTicket(ctx: CanvasRenderingContext2D): void {

    ctx.beginPath();

    ctx.clearRect(0, 0, this.CANVAS_WIDTH, this.CANVAS_HEIGHT);

    if (!this.formComplete()) {
      return;
    }

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
    ctx.fillText("Date: " + new Date(Date.parse(this.guestTicket.ticketValidOn!.toString())).toDateString() , (MARGIN + 9) * this.SCALE, (MARGIN + 70) * this.SCALE);

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
    this.guestTicket.imageData = this.canvas.nativeElement.toDataURL();
    //console.log(this.guestTicket.imageData);
  }

  addLeadingZeros(num: number, totalLength: number): string {
    return String(num).padStart(totalLength, '0');
  }

  private setCanvasSCALE() {
    // if (isPortait) {
    //   this.SCALE = ((this.screenService.Width * .95)  - 4 - (MARGIN *2)) / TICKET_WIDTH;
    // } else {
      //this.SCALE = 3.5;
      var verticalFormUsed = 360; // Pixels taken up by form so remainder for ticket
      if (this.screenService.Height < 500) {
        verticalFormUsed = 50;
      }
      this.SCALE = ((this.screenService.Height * .9) - verticalFormUsed - 4 - (MARGIN *2)) / TICKET_HEIGHT;
      
     this.SCALE = 3.8;
      //this.SCALE = 10;
      //this.SCALE = 0;
    // }

    this.CANVAS_WIDTH  = (TICKET_WIDTH * this.SCALE) + (MARGIN * this.SCALE * 2);
    this.CANVAS_HEIGHT = (TICKET_HEIGHT * this.SCALE) + (MARGIN * this.SCALE * 2);
  
    this.canvas.nativeElement.width = this.CANVAS_WIDTH;
    this.canvas.nativeElement.height = this.CANVAS_HEIGHT;

    console.log(`${new Date().toLocaleTimeString()} - setting scale to: ${this.SCALE}, dialogWIdth: ${this.dialogWidth}, dialogHeight: ${this.dialogHeight}, screenAspectRatio: ${this.screenAspectRatio}`);
    this.previewTicket();
  }

  private get screenAspectRatio() :number {return this.screenWidth / this.screenHeight};
  private get ticketAvailableAspectRatio() :number {return this.ticketWidth / this.ticketHeight};

}


