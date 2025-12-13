import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { WelcomeComponent } from './components/welcome/welcome.component';
import { NewsComponent } from './components/news/news.component';
import { WatersComponent } from './components/waters/waters.component';
import { MatchesComponent } from './components/matches/matches.component';
import { PhotosComponent } from './components/photos/photos.component';
// import { RulesComponent } from './components/rules/rules.component';
import { DiaryComponent } from './components/diary/diary.component';
import { LeagueStandingsComponent } from './components/league-standings/league-standings.component';
import { AggregateWeightsComponent } from './components/aggregate-weights/aggregate-weights.component';
import { LoginComponent } from './components/login/login.component';
import { AuthGuard } from './services/auth/auth.guard';
import { LogoutComponent } from './components/logout/logout.component';
import { MemberComponent } from './components/member/member.component';
import { MembersComponent } from './components/members/members.component';
import { MyDetailsComponent } from './components/my-details/my-details.component';
import { UserAdminsComponent } from './components/user-admins/user-admins.component';
import { DeactivateGuardService } from './services/deactivate-guard.service';
import { ClubOfficersComponent } from './components/club-officers/club-officers.component';
import { FormsComponent } from './components/forms/forms.component';
import { AnglingTrustComponent } from './components/angling-trust/angling-trust.component';
import { EnvironmentalComponent } from './components/environmental/environmental.component';
import { RulesGeneralComponent } from './components/rules-general/rules-general.component';
import { RulesMatchComponent } from './components/rules-match/rules-match.component';
import { RulesJnrGeneralComponent } from './components/rules-jnr-general/rules-jnr-general.component';
import { RulesJnrMatchComponent } from './components/rules-jnr-match/rules-jnr-match.component';
import { GuestTicketCreateComponent } from './components/guest-ticket-create/guest-ticket-create.component';
import { GuestTicketsComponent } from './components/guest-tickets/guest-tickets.component';
import { BuyMembershipsComponent } from './components/buy-memberships/buy-memberships.component';
import { PaymentsComponent } from './components/payments/payments.component';
import { BuyDayTicketsComponent } from './components/buy-day-tickets/buy-day-tickets.component';
import { BuyGuestTicketsComponent } from './components/buy-guest-tickets/buy-guest-tickets.component';
import { BuySuccessComponent } from './components/buy-success/buy-success.component';
import { TermsAndConditionsComponent } from './components/terms-and-conditions/terms-and-conditions.component';
import { DataPolicyComponent } from './components/data-policy/data-policy.component';
import { JuniorProgrammeIntroComponent } from './components/junior-programme-intro/junior-programme-intro.component';
import { JuniorTrophiesComponent } from './components/junior-trophies/junior-trophies.component';
import { JuniorTacklePleaComponent } from './components/junior-tackle-plea/junior-tackle-plea.component';
import { JuniorMatchesComponent } from './components/junior-matches/junior-matches.component';
import { JuniorSponsorsComponent } from './components/junior-sponsors/junior-sponsors.component';
import { TrophyWinnersComponent } from './components/trophy-winners/trophy-winners.component';
import { RegistrationsComponent } from './components/registrations/registrations.component';
import { RegisterComponent } from './components/register/register.component';
import { JuniorOpenRegistrationsComponent } from './components/junior-open-registrations/junior-open-registrations.component';
import { GenerateBookPrintingPagesComponent } from './components/generate-book-printing-pages/generate-book-printing-pages.component';
import { BlazorHostComponent } from './components/blazor-host/blazor-host.component';

const routes: Routes = [
  { path: '', redirectTo: '/welcome', pathMatch: 'full'},
  { path: 'login', component: LoginComponent, canDeactivate:[DeactivateGuardService]  },
  { path: 'logout', component: LogoutComponent },
  
  // { path: 'welcome', component: WelcomeComponent },  // Angular welcome page
  { path: '', component: BlazorHostComponent },         // Blazor welcome page

  { path: 'welcome', component: WelcomeComponent },
  { path: 'welcome', component: WelcomeComponent },
  { path: 'news', component: NewsComponent },
  { path: 'waters', component: WatersComponent },
  { path: 'officers', component: ClubOfficersComponent },
  { path: 'forms', component: FormsComponent },
  { path: 'anglingTrust', component: AnglingTrustComponent },
  { path: 'environmental', component: EnvironmentalComponent },
  { path: 'matches', component: MatchesComponent, canActivate: [AuthGuard] },
  { path: 'standings', component: LeagueStandingsComponent, canActivate: [AuthGuard] },
  { path: 'aggregateWeights', component: AggregateWeightsComponent, canActivate: [AuthGuard] },
  { path: 'photos', component: PhotosComponent, canActivate: [AuthGuard] },
  // { path: 'rules/:type', component: RulesComponent, canActivate: [AuthGuard]  },
  { path: 'diary', component: DiaryComponent, canActivate: [AuthGuard] },
  { path: 'member/:id', component: MemberComponent, canActivate: [AuthGuard] },
  { path: 'members', component: MembersComponent, canActivate: [AuthGuard] },
  { path: 'myDetails', component: MyDetailsComponent, canActivate: [AuthGuard] },
  { path: 'userAdmins', component: UserAdminsComponent, canActivate: [AuthGuard] },
  { path: 'termsAndConditions', component: TermsAndConditionsComponent},
  { path: 'dataPolicy', component: DataPolicyComponent},
  { path: 'rulesGeneral', component: RulesGeneralComponent, canActivate: [AuthGuard] },
  { path: 'rulesMatch', component: RulesMatchComponent, canActivate: [AuthGuard] },
  { path: 'rulesJnrGeneral', component: RulesJnrGeneralComponent, canActivate: [AuthGuard] },
  { path: 'rulesJnrMatch', component: RulesJnrMatchComponent, canActivate: [AuthGuard] },
  { path: 'guestTickets', component: GuestTicketsComponent, canActivate: [AuthGuard] },
  { path: 'guestTicketCreate', component: GuestTicketCreateComponent, canActivate: [AuthGuard] },
  { path: 'buyMemberships', component: BuyMembershipsComponent},
  { path: 'payments', component: PaymentsComponent, canActivate: [AuthGuard] },
  { path: 'buyDayTickets', component: BuyDayTicketsComponent },
  // { path: 'buyPondGateKeys', component: BuyPondGateKeysComponent }, // Now included as part of membership payment
  { path: 'buyGuestTickets', component: BuyGuestTicketsComponent, canActivate: [AuthGuard] },
  { path: 'buySuccess/:productType', component: BuySuccessComponent },
  { path: 'juniorProgrammeIntro', component: JuniorProgrammeIntroComponent},
  { path: 'juniorTrophies', component: JuniorTrophiesComponent},
  { path: 'juniorTacklePlea', component: JuniorTacklePleaComponent},
  { path: 'juniorSponsors', component: JuniorSponsorsComponent},
  { path: 'juniorMatches', component: JuniorMatchesComponent, canActivate: [AuthGuard] },
  { path: 'trophyWinners', component: TrophyWinnersComponent, canActivate: [AuthGuard] },
  { path: 'registrations', component: RegistrationsComponent},
  { path: 'register', component: RegisterComponent},
  { path: 'juniorOpenRegistrations', component: JuniorOpenRegistrationsComponent},
  { path: 'generateBookPrintingPages', component: GenerateBookPrintingPagesComponent},

 // Single host route for “Blazor area”
  { path: 'blazor', component: BlazorHostComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
