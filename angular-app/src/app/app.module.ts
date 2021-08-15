import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavComponent } from './components/nav/nav.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { FormsModule, ReactiveFormsModule} from '@angular/forms';

import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ErrorIntercept } from './services/error.interceptor';

import { MatSidenavModule} from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { AgmCoreModule } from '@agm/core';

import { WelcomeComponent } from './components/welcome/welcome.component';
import { NewsComponent } from './components/news/news.component';
import { WatersComponent } from './components/waters/waters.component';
import { MatchesComponent } from './components/matches/matches.component';
import { PhotosComponent } from './components/photos/photos.component';
import { RulesComponent } from './components/rules/rules.component';
import { PreviewComponent } from './components/preview/preview.component';
import { MatchInfoComponent } from './dialogs/match-info/match-info.component';
import { DiaryComponent } from './components/diary/diary.component';
import { ErrorComponent } from './dialogs/error/error.component';
import { LeagueStandingsComponent } from './components/league-standings/league-standings.component';
import { AggregateWeightsComponent } from './components/aggregate-weights/aggregate-weights.component';
import { ConfirmDialogComponent } from './dialogs/confirm-dialog/confirm-dialog.component';
import { AddEditNewsItemDialogComponent } from './dialogs/add-edit-news-item-dialog/add-edit-news-item-dialog.component';
import { MatPaginatorModule } from '@angular/material/paginator';

// Rich text editor
import { FateModule, FateMaterialModule } from 'fate-editor';
import { LoginComponent } from './components/login/login.component';
import { JwtInterceptor } from './services/auth/jwt.Interceptor';
import { LogoutComponent } from './components/logout/logout.component';
import { LoginPreferencesDialogComponent } from './dialogs/login-preferences-dialog/login-preferences-dialog.component';
import { MemberComponent } from './components/member/member.component';
import { MembersComponent } from './components/members/members.component';
import { MyDetailsComponent } from './components/my-details/my-details.component';
import { UserAdminsComponent } from './components/user-admins/user-admins.component';
import { AddEditUserAdminDialogComponent } from './dialogs/add-edit-user-admin-dialog/add-edit-user-admin-dialog.component';
import { MatSortModule } from '@angular/material/sort';
import { ResetPinComponent } from './dialogs/reset-pin/reset-pin.component';

@NgModule({
  declarations: [
    AppComponent,
    NavComponent,
    WelcomeComponent,
    NewsComponent,
    WatersComponent,
    MatchesComponent,
    PhotosComponent,
    RulesComponent,
    PreviewComponent,
    MatchInfoComponent,
    DiaryComponent,
    ErrorComponent,
    LeagueStandingsComponent,
    AggregateWeightsComponent,
    ConfirmDialogComponent,
    AddEditNewsItemDialogComponent,
    LoginComponent,
    LogoutComponent,
    LoginPreferencesDialogComponent,
    MemberComponent,
    MembersComponent,
    MyDetailsComponent,
    UserAdminsComponent,
    AddEditUserAdminDialogComponent,
    ResetPinComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,

    FormsModule, 
    ReactiveFormsModule,

    AgmCoreModule.forRoot({
      apiKey: 'AIzaSyDHevRsT--n12LBiAdO034OFHV7WCJVWck'
    }),

    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatChipsModule,
    MatListModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatDialogModule,
    MatTabsModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,

    FateModule,
    FateMaterialModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorIntercept,
      multi: true,
      deps: [MatDialog]
    },
    { 
      provide: HTTP_INTERCEPTORS, 
      useClass: JwtInterceptor, 
      multi: true 
    },
    { 
      provide: MAT_DATE_LOCALE, 
      useValue: 'en-GB' },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
