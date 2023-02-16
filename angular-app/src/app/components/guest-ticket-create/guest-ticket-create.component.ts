import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { GuestTicket } from 'src/app/models/guest-ticket';

const SCALE = 3.5;

// Sizes in mm
const MARGIN = 10;         
const TICKET_WIDTH  = 173; 
const TICKET_HEIGHT = 82;  
const VERTICAL_DIVIDER = 60;
const PRICE_DIVIDER = 19;
const TICKET_NUMBER_DIVIDER = 55;

const CANVAS_WIDTH  = (TICKET_WIDTH * SCALE) + (MARGIN * SCALE * 2);
const CANVAS_HEIGHT = (TICKET_HEIGHT * SCALE) + (MARGIN * SCALE * 2);

@Component({
  selector: 'app-guest-ticket-create',
  templateUrl: './guest-ticket-create.component.html',
  styleUrls: ['./guest-ticket-create.component.css']
})
export class GuestTicketCreateComponent implements OnInit, AfterViewInit {

  @ViewChild('canvasEl', { static: false }) canvas!: ElementRef<HTMLCanvasElement>; 

  private context!: CanvasRenderingContext2D;

  private guestTicket!: GuestTicket;

  constructor() { }

  ngAfterViewInit() {

    this.canvas.nativeElement.width = CANVAS_WIDTH;
    this.canvas.nativeElement.height = CANVAS_HEIGHT;
    // this.canvas.nativeElement.style.backgroundColor = "Yellow";

    this.context = <CanvasRenderingContext2D>(
      this.canvas.nativeElement as HTMLCanvasElement
    ).getContext("2d");

    this.drawTicket(this.context);
  }

  ngOnInit(): void {
    
    this.guestTicket = new GuestTicket();

    this.guestTicket.cost = 5;
    this.guestTicket.ticketNumber = 1;
    this.guestTicket.issuedBy = "S.Townend";
    this.guestTicket.issuedOn = new Date("2022-03-13");
    this.guestTicket.ticketValidOn = new Date("2022-04-07");
    this.guestTicket.membersName = "F.Spencer";
    this.guestTicket.guestsName = "N.Wisdom";
  }

  drawTicket(ctx: CanvasRenderingContext2D): void {
    ctx.beginPath();

    ctx.fillStyle = "#FFFFFF";
    ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

    ctx.fillStyle = "#000000";
    ctx.rect(MARGIN * SCALE, MARGIN * SCALE, TICKET_WIDTH * SCALE, TICKET_HEIGHT * SCALE);

    ctx.moveTo((MARGIN + VERTICAL_DIVIDER) * SCALE, MARGIN * SCALE);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * SCALE, (MARGIN + TICKET_HEIGHT) * SCALE);

    ctx.moveTo(MARGIN * SCALE, (MARGIN + PRICE_DIVIDER) * SCALE);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * SCALE,  (MARGIN + PRICE_DIVIDER) * SCALE);

    ctx.moveTo(MARGIN * SCALE, (MARGIN + TICKET_NUMBER_DIVIDER) * SCALE);
    ctx.lineTo((MARGIN + VERTICAL_DIVIDER) * SCALE,  (MARGIN + TICKET_NUMBER_DIVIDER) * SCALE);

    // e.g. context.font="italic small-caps bold 12px arial";
    ctx.font="normal normal normal 16px arial";

    ctx.textAlign = "center";
    ctx.fillText("£" + (Math.round(this.guestTicket.cost * 100) / 100).toFixed(2), (MARGIN + (VERTICAL_DIVIDER / 2)) * SCALE, (MARGIN + 11) * SCALE);

    ctx.fillText("TICKET NO.", (MARGIN + (VERTICAL_DIVIDER / 2)) * SCALE, (MARGIN + 26) * SCALE);
    ctx.fillText("On line/" + this.addLeadingZeros(this.guestTicket.ticketNumber, 4), (MARGIN + (VERTICAL_DIVIDER / 2)) * SCALE, (MARGIN + 33) * SCALE);

    ctx.font="normal normal normal 12px arial";

    ctx.textAlign = "left";
    ctx.fillText("Issued by: " + this.guestTicket.issuedBy, (MARGIN + 9) * SCALE, (MARGIN + 42) * SCALE);
    ctx.fillText("Issued on: " + this.guestTicket.issuedOn.toDateString(), (MARGIN + 9) * SCALE, (MARGIN + 48) * SCALE);

    ctx.textAlign = "center";
    ctx.fillText("Ticket Covers:", (MARGIN + (VERTICAL_DIVIDER / 2)) * SCALE, (MARGIN + 60) * SCALE);

    ctx.textAlign = "left";
    ctx.fillText("Date: " + this.guestTicket.ticketValidOn!.toDateString() , (MARGIN + 9) * SCALE, (MARGIN + 70) * SCALE);

    ctx.textAlign = "center";
    ctx.font="normal normal bold 14px arial";
    ctx.fillText("BOROUGHBRIDGE & DISTRICT ANGLING CLUB", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * SCALE, (MARGIN + 10) * SCALE);

    ctx.font="normal normal bold 18px arial";
    ctx.fillText("MEMBERS GUEST TICKET", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * SCALE, (MARGIN + 25) * SCALE);

    ctx.font="normal normal normal 12px arial";
    ctx.fillText("for the", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * SCALE, (MARGIN + 34) * SCALE);
    ctx.font="normal normal normal 14px arial";
    ctx.fillText("CLUB WATERS: RIVER URE & ROECLIFFE POND", (MARGIN + (TICKET_WIDTH - ((TICKET_WIDTH - VERTICAL_DIVIDER) / 2))) * SCALE, (MARGIN + 40) * SCALE);

    ctx.textAlign = "left";
    ctx.fillText("MEMBERS NAME: " + this.guestTicket.membersName, (MARGIN + VERTICAL_DIVIDER + 9) * SCALE, (MARGIN + 50) * SCALE);
    ctx.fillText("GUESTS NAME: " + this.guestTicket.guestsName, (MARGIN + VERTICAL_DIVIDER + 9) * SCALE, (MARGIN + 58) * SCALE);

    ctx.textAlign = "left";
    ctx.font="normal normal normal 12px arial";
    ctx.fillText("NO TICKETS AVAILABLE ON ANY SUNDAY MATCH VENUES", (MARGIN + VERTICAL_DIVIDER + 9) * SCALE, (MARGIN + 70) * SCALE);
    ctx.font="normal normal normal 10px arial";
    var noticeText = "Please read the rules and bait bans on the notice board ";
    var noticeTextWidth = ctx.measureText(noticeText).width;
    ctx.fillText(noticeText, (MARGIN + VERTICAL_DIVIDER + 9) * SCALE, (MARGIN + 74) * SCALE);

    ctx.fillStyle = "#FF0000";
    ctx.font="normal normal bold 10px arial";
    ctx.fillText("before", ((MARGIN + VERTICAL_DIVIDER + 9) * SCALE) + noticeTextWidth, (MARGIN + 74) * SCALE);
    var before = "before";
    var beforeWidth = ctx.measureText(before).width;
    ctx.moveTo(((MARGIN + VERTICAL_DIVIDER + 9) * SCALE) + noticeTextWidth,  ((MARGIN + 74) * SCALE) + 2);
    ctx.lineTo(((MARGIN + VERTICAL_DIVIDER + 9) * SCALE) + noticeTextWidth + beforeWidth,  ((MARGIN + 74) * SCALE) + 2);

    ctx.fillStyle = "#000000";
    ctx.font="normal normal normal 10px arial";
    var noticeTextBefore = noticeText + "before ";
    var noticeTextBeforeWidth = ctx.measureText(noticeTextBefore).width + 2;
    ctx.fillText("fishing.", ((MARGIN + VERTICAL_DIVIDER + 9) * SCALE) + noticeTextBeforeWidth, (MARGIN + 74) * SCALE);

    //ctx.fillText("Please read the rules and bait bans on the notice board before fishing.", (MARGIN + VERTICAL_DIVIDER + 9) * SCALE, (MARGIN + 74) * SCALE);
    ctx.fillText("Members must fish with their guest and be responsible for them.", (MARGIN + VERTICAL_DIVIDER + 9) * SCALE, (MARGIN + 78) * SCALE);

    ctx.stroke();
    
    //var dataUrl = this.canvas.nativeElement.toDataURL('image/jpeg', 1.0);
    var dataUrl = this.canvas.nativeElement.toDataURL();
    console.log(dataUrl);
  }

  addLeadingZeros(num: number, totalLength: number): string {
    return String(num).padStart(totalLength, '0');
  }
}
