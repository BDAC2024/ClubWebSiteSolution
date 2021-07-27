import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/models/member';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member',
  templateUrl: './member.component.html',
  styleUrls: ['./member.component.css']
})
export class MemberComponent implements OnInit {

  private membershipNumber!: number;
  public member!: Member;
  public isLoading: boolean = false;
  public status: string = "";

  constructor(private route: ActivatedRoute,
    private membersService: MembersService) { 
    this.route.params.subscribe(params => {
      console.log("Member number: " + params.membershipNumber);

      this.isLoading = true;

      this.membersService.readMember(params.membershipNumber)
        .subscribe(data => {
          this.member = data;
          this.isLoading = false;
        });
    })
  }

  ngOnInit(): void {
  }

  public save(): void {
    this.membersService.updateMember(this.member)
    .subscribe(data => {
      this.status = "Saved successfully";
    });

  }

  
}
