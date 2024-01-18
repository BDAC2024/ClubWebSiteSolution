import { Component, OnInit } from '@angular/core';
import { MembershipPayment } from 'src/app/models/membership-payment';
import { Observable } from 'rxjs';
import { MatTableDataSource } from '@angular/material/table';

const DATA: MembershipPayment[] = [
  {
    type: "Adult",
    term: "12 months",
    cost: 52,
    buttonId: "buy_btn_1N82AyI81Rrb3iDAUF7vAG18",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_14k9Cr7vTfs3eSAeUU",
    notes: ""
  },
  {
    type: "Junior - 12 to 15 years of age (at 1st of April)",
    term: "12 months",
    cost: 12,
    buttonId: "buy_btn_1N82VbI81Rrb3iDAynfehWtM",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_fZeaGv8zX6VxdOwaEF",
    notes: "11 and under are FREE"
  },
  {
    type: "Intermediate - 16 to 18 years of age (at 1st of April)",
    term: "12 months",
    cost: 27,
    buttonId: "buy_btn_1N83JII81Rrb3iDAY7x7Zkzt",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_7sIdSHbM95Rt11K147",
    notes: ""
  },
  {
    type: "Senior Citizen",
    term: "12 months",
    cost: 32,
    buttonId: "buy_btn_1N82N8I81Rrb3iDAvgrQwa6n",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_4gw5mbdUha7J25O3ce",
    notes: ""
  },
  {
    type: "Disabled",
    term: "12 months",
    cost: 27,
    buttonId: "buy_btn_1N83KcI81Rrb3iDAP4ZYeJlP",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_6oEdSH3fDcfR8uc148",
    notes: ""
  },
  {
    type: "Adult",
    term: "6 months",
    cost: 32,
    buttonId: "buy_btn_1N83LXI81Rrb3iDAcXr8ljbJ",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_5kAeWL7vT2Fh8uc3ch",
    notes: ""
  },
  {
    type: "Juniors - 12 to 18 years of age (at 1st of April)",
    term: "6 months",
    cost: 7,
    buttonId: "buy_btn_1N83MjI81Rrb3iDAMed02IhU",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_fZedSH03r6Vx8ucfZ4",
    notes: "11 and under are FREE"
  },
  {
    type: "Disabled",
    term: "6 months",
    cost: 27,
    buttonId: "buy_btn_1N83NfI81Rrb3iDAN86Iyyt6",
    key: "pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx",
    link: "https://buy.stripe.com/test_dR67uj03r6Vx6m4eV1",
    notes: ""
  }
];

@Component({
  selector: 'app-buy-memberships',
  templateUrl: './buy-memberships.component.html',
  styleUrls: ['./buy-memberships.component.css']
})
export class BuyMembershipsComponent implements OnInit {

  obs!: Observable<any>;
  dataSource: MatTableDataSource<MembershipPayment> = new MatTableDataSource<MembershipPayment>(DATA);

  public displayedColumns: string[] = ["type", "term", "cost", "action"];
  public memberships: MembershipPayment[] = [];
  public isLoading: boolean = true;

  constructor() { }

  ngOnInit(): void {
    this.obs = this.dataSource.connect();

    this.isLoading = false;
  }

  public buy(membership: MembershipPayment): void {
    window.open(membership.link, '_blank');
  }
}
