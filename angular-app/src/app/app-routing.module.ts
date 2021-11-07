import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { WelcomeComponent } from './components/welcome/welcome.component';
import { NewsComponent } from './components/news/news.component';
import { WatersComponent } from './components/waters/waters.component';
import { MatchesComponent } from './components/matches/matches.component';
import { PhotosComponent } from './components/photos/photos.component';
import { RulesComponent } from './components/rules/rules.component';
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

const routes: Routes = [
  { path: '', redirectTo: '/welcome', pathMatch: 'full'},
  { path: 'login', component: LoginComponent, canDeactivate:[DeactivateGuardService]  },
  { path: 'logout', component: LogoutComponent },
  { path: 'welcome', component: WelcomeComponent },
  { path: 'news', component: NewsComponent },
  { path: 'waters', component: WatersComponent },
  { path: 'officers', component: ClubOfficersComponent },
  { path: 'matches', component: MatchesComponent, canActivate: [AuthGuard] },
  { path: 'standings', component: LeagueStandingsComponent, canActivate: [AuthGuard] },
  { path: 'aggregateWeights', component: AggregateWeightsComponent, canActivate: [AuthGuard] },
  { path: 'photos', component: PhotosComponent, canActivate: [AuthGuard] },
  { path: 'rules/:type', component: RulesComponent, canActivate: [AuthGuard]  },
  { path: 'diary', component: DiaryComponent, canActivate: [AuthGuard] },
  { path: 'member/:id', component: MemberComponent, canActivate: [AuthGuard] },
  { path: 'members', component: MembersComponent, canActivate: [AuthGuard] },
  { path: 'myDetails', component: MyDetailsComponent, canActivate: [AuthGuard] },
  { path: 'userAdmins', component: UserAdminsComponent, canActivate: [AuthGuard] },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
