import { AfterViewInit, Component, ElementRef, Inject, NgZone, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { MAT_DATE_FORMATS } from '@angular/material/core';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';
import { RefData } from 'src/app/models/refData';
import { RefDataService } from 'src/app/services/ref-data.service';
import { OLDGuestTicket } from 'src/app/models/guest-ticket';
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
const TICKET_HEIGHT = 86;  
const VERTICAL_DIVIDER = 60;
const PRICE_DIVIDER = 19;
const TICKET_NUMBER_DIVIDER = 55;

const DIALOG_USED_HEIGHT = 392; // pixels
const IDEAL_DIALOG_WIDTH = 672;
const IDEAL_DIALOG_HEIGHT = 715;

@Component({
  selector: 'app-create-guest-ticket',
  templateUrl: './create-guest-ticket.component.html',
  styleUrls: ['./create-guest-ticket.component.css']
})

export class CreateGuestTicketComponent implements OnInit, OnDestroy, AfterViewInit {
  myControl: UntypedFormControl = new UntypedFormControl();
  emailControl: UntypedFormControl = new UntypedFormControl();
  
  private VIEWING_SCALE = 3.8;
  private PRINTING_SCALE = 10;

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

  //public guestTicket!: OLDGuestTicket;

  @ViewChild('viewingCanvasEl', { static: false }) viewingCanvas!: ElementRef<HTMLCanvasElement>; 
  @ViewChild('printingCanvasEl', { static: false }) printingCanvas!: ElementRef<HTMLCanvasElement>; 

  private viewingContext!: CanvasRenderingContext2D;
  private printingContext!: CanvasRenderingContext2D;

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
    
    @Inject(MAT_DIALOG_DATA) public guestTicket: OLDGuestTicket) {
      if (guestTicket.dbKey != "") {
        this.title = "Edit Guest Ticket";
        this.editMode = true;
      } else {
        this.title = "Issue Guest Ticket";
      }

      this.emailTo = guestTicket.emailTo;
      this.emailControl.setValue(this.emailTo);

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

    this.setCanvasElementSize();
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

    //this.guestTicket = new OLDGuestTicket();
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

    this.viewingContext = <CanvasRenderingContext2D>(
      this.viewingCanvas.nativeElement as HTMLCanvasElement
    ).getContext("2d");

    this.printingContext = <CanvasRenderingContext2D>(
      this.printingCanvas.nativeElement as HTMLCanvasElement
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
    this.guestTicket.emailTo = this.emailTo;
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
    if (this.selectedMember != null) {
      this.guestTicket.membersName = this.selectedMember?.name;
      this.guestTicket.membershipNumber = this.selectedMember?.membershipNumber;
    }
    if (this.emailTo) {
      this.guestTicket.emailTo = this.emailTo;
    }

    this.drawTicket(this.viewingContext, this.VIEWING_SCALE);
    this.drawTicket(this.printingContext, this.PRINTING_SCALE);
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

  drawTicket(ctx: CanvasRenderingContext2D, scale: number): void {

    ctx.beginPath();

    var canvasWidth  = (TICKET_WIDTH * scale) + (MARGIN * scale * 2);
    var canvasHeight = (TICKET_HEIGHT * scale) + (MARGIN * scale * 2);

    ctx.clearRect(0, 0, canvasWidth, canvasHeight);

    if (!this.formComplete()) {
      return;
    }

    ctx.fillStyle = "#FFFFFF";
    ctx.fillRect(0, 0, canvasWidth, canvasHeight);

    ctx.fillStyle = "#000000";
    ctx.rect(MARGIN * scale, MARGIN * scale, TICKET_WIDTH * scale, TICKET_HEIGHT * scale);

    ctx.moveTo((MARGIN + VERTICAL_DIVIDER) * scale, MARGIN * scale);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * scale, (MARGIN + TICKET_HEIGHT) * scale);

    ctx.moveTo(MARGIN * scale, (MARGIN + PRICE_DIVIDER) * scale);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * scale,  (MARGIN + PRICE_DIVIDER) * scale);

    ctx.moveTo(MARGIN * scale, (MARGIN + TICKET_NUMBER_DIVIDER) * scale);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * scale,  (MARGIN + TICKET_NUMBER_DIVIDER) * scale);

    // e.g. context.font="italic small-caps bold 12px arial";
    ctx.font=`normal normal normal ${16 / 3.5 * scale}px arial`;

    ctx.textAlign = "center";
    ctx.fillText("£" + (Math.round(this.guestTicket.cost * 100) / 100).toFixed(2), (MARGIN + (VERTICAL_DIVIDER / 2)) * scale, (MARGIN + 11) * scale);

    ctx.fillText("TICKET NO.", (MARGIN + (VERTICAL_DIVIDER / 2)) * scale, (MARGIN + 26) * scale);
    ctx.fillText((this.guestTicket.ticketNumber == null ? "Generated when sent" : ("On line/" + this.addLeadingZeros(this.guestTicket.ticketNumber, 4))), (MARGIN + (VERTICAL_DIVIDER / 2)) * scale, (MARGIN + 33) * scale);

    ctx.font=`normal normal normal ${12 / 3.5 * scale}px arial`;

    ctx.textAlign = "left";
    ctx.fillText("Issued by: " + this.guestTicket.issuedBy, (MARGIN + 9) * scale, (MARGIN + 42) * scale);
    ctx.fillText("Issued on: " + this.guestTicket.issuedOn.toDateString(), (MARGIN + 9) * scale, (MARGIN + 48) * scale);

    ctx.textAlign = "center";
    ctx.fillText("Ticket Covers:", (MARGIN + (VERTICAL_DIVIDER / 2)) * scale, (MARGIN + 60) * scale);

    ctx.textAlign = "left";
    ctx.fillText("Date: " + new Date(Date.parse(this.guestTicket.ticketValidOn!.toString())).toDateString() , (MARGIN + 9) * scale, (MARGIN + 70) * scale);

    ctx.textAlign = "center";
    ctx.font=`normal normal bold ${14 / 3.5 * scale}px arial`;
    ctx.fillText("BOROUGHBRIDGE & DISTRICT ANGLING CLUB", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * scale, (MARGIN + 10) * scale);

    ctx.font=`normal normal bold ${18 / 3.5 * scale}px arial`;
    ctx.fillText("MEMBERS GUEST TICKET", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * scale, (MARGIN + 25) * scale);

    ctx.font=`normal normal normal ${12 / 3.5 * scale}px arial`;
    ctx.fillText("for the", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * scale, (MARGIN + 34) * scale);
    ctx.font=`normal normal normal ${14 / 3.5 * scale}px arial`;
    ctx.fillText("CLUB WATERS: RIVER URE & ROECLIFFE POND", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * scale, (MARGIN + 40) * scale);

    ctx.textAlign = "left";
    ctx.fillText("MEMBERS NAME: " + this.guestTicket.membersName, (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 50) * scale);
    ctx.fillText("GUESTS NAME: " + this.guestTicket.guestsName, (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 58) * scale);

    ctx.textAlign = "left";
    ctx.font=`normal normal normal ${12 / 3.5 * scale}px arial`;
    ctx.fillText("NO TICKETS AVAILABLE ON ANY SUNDAY MATCH VENUES", (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 70) * scale);
    ctx.font=`normal normal normal ${10 / 3.5 * scale}px arial`;
    var noticeText = "Please read the pond rules and bait bans on the notice board ";
    var noticeTextWidth = ctx.measureText(noticeText).width;
    ctx.fillText(noticeText, (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 74) * scale);

    ctx.fillStyle = "#FF0000";
    ctx.font=`normal normal bold ${10 / 3.5 * scale}px arial`;
    ctx.fillText("before", ((MARGIN + VERTICAL_DIVIDER + 9) * scale) + noticeTextWidth, (MARGIN + 74) * scale);
    var before = "before";
    var beforeWidth = ctx.measureText(before).width;
    ctx.moveTo(((MARGIN + VERTICAL_DIVIDER + 9) * scale) + noticeTextWidth,  ((MARGIN + 74) * scale) + 2);
    ctx.lineTo(((MARGIN + VERTICAL_DIVIDER + 9) * scale) + noticeTextWidth + beforeWidth,  ((MARGIN + 74) * scale) + 2);

    ctx.fillStyle = "#000000";
    ctx.font=`normal normal normal ${10 / 3.5 * scale}px arial`;
    var noticeTextBefore = noticeText + "before ";
    var noticeTextBeforeWidth = ctx.measureText(noticeTextBefore).width + 2;
    ctx.fillText("fishing.", ((MARGIN + VERTICAL_DIVIDER + 9) * scale) + noticeTextBeforeWidth, (MARGIN + 74) * scale);

    //ctx.fillText("Please read the pond rules and bait bans on the notice board before fishing.", (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 74) * scale);
    ctx.fillText("Members must fish with their guest and be responsible for them.", (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 78) * scale);
    ctx.fillText("NO FISHING from boats either moving or static/moored.", (MARGIN + VERTICAL_DIVIDER + 9) * scale, (MARGIN + 82) * scale);

    ctx.stroke();
    
    //var dataUrl = this.canvas.nativeElement.toDataURL('image/jpeg', 1.0);
    if (ctx == this.printingContext) {
      this.guestTicket.imageData = this.printingCanvas.nativeElement.toDataURL();
    }
    //console.log(this.guestTicket.imageData);
  }

  addLeadingZeros(num: number, totalLength: number): string {
    return String(num).padStart(totalLength, '0');
  }

  private setCanvasElementSize() {

    var viewingCanvasWidth  = (TICKET_WIDTH * this.VIEWING_SCALE) + (MARGIN * this.VIEWING_SCALE * 2);
    var viewingCanvasHeight = (TICKET_HEIGHT * this.VIEWING_SCALE) + (MARGIN * this.VIEWING_SCALE * 2);
    this.viewingCanvas.nativeElement.width = viewingCanvasWidth;
    this.viewingCanvas.nativeElement.height = viewingCanvasHeight;

    var printingCanvasWidth  = (TICKET_WIDTH * this.PRINTING_SCALE) + (MARGIN * this.PRINTING_SCALE * 2);
    var printingCanvasHeight = (TICKET_HEIGHT * this.PRINTING_SCALE) + (MARGIN * this.PRINTING_SCALE * 2);
    this.printingCanvas.nativeElement.width = printingCanvasWidth;
    this.printingCanvas.nativeElement.height = printingCanvasHeight;

    this.previewTicket();
  }

  private get screenAspectRatio() :number {return this.screenWidth / this.screenHeight};
  private get ticketAvailableAspectRatio() :number {return this.ticketWidth / this.ticketHeight};

}


