import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, MAT_DATE_LOCALE } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatBadgeModule} from '@angular/material/badge';
import { AgmCoreModule } from '@hudsontavares/agm-core';

import { WelcomeComponent } from './components/welcome/welcome.component';
import { NewsComponent } from './components/news/news.component';
import { WatersComponent } from './components/waters/waters.component';
import { MatchesComponent } from './components/matches/matches.component';
import { PhotosComponent } from './components/photos/photos.component';
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
import { MatAutocomplete, MatAutocompleteModule } from '@angular/material/autocomplete';
import { ResetPinComponent } from './dialogs/reset-pin/reset-pin.component';
import { AddEditWaterDialogComponent } from './dialogs/add-edit-water-dialog/add-edit-water-dialog.component';
import { AddEditRulesDialogComponent } from './dialogs/add-edit-rules-dialog/add-edit-rules-dialog.component';
import { AuthenticationService } from './services/auth/authentication.service';
import { Router } from '@angular/router';
import { MatSelectModule } from '@angular/material/select';
import { ScrollingModule } from '@angular/cdk/scrolling';
import { ClubOfficersComponent } from './components/club-officers/club-officers.component';
import { FormsComponent } from './components/forms/forms.component';
import { AnglingTrustComponent } from './components/angling-trust/angling-trust.component';
import { EnvironmentalComponent } from './components/environmental/environmental.component';
import { AddMemberComponent } from './dialogs/add-member/add-member.component';
import { RulesGeneralComponent } from './components/rules-general/rules-general.component';
import { RulesMatchComponent } from './components/rules-match/rules-match.component';
import { RulesJnrGeneralComponent } from './components/rules-jnr-general/rules-jnr-general.component';
import { RulesJnrMatchComponent } from './components/rules-jnr-match/rules-jnr-match.component';
import { GuestTicketCreateComponent } from './components/guest-ticket-create/guest-ticket-create.component';
import { GuestTicketsComponent } from './components/guest-tickets/guest-tickets.component';
import { CreateGuestTicketComponent } from './dialogs/create-guest-ticket/create-guest-ticket.component';
import { BuyMembershipsComponent } from './components/buy-memberships/buy-memberships.component';
import { PaymentsComponent } from './components/payments/payments.component';
import { BuyDayTicketsComponent } from './components/buy-day-tickets/buy-day-tickets.component';
import { BuyGuestTicketsComponent } from './components/buy-guest-tickets/buy-guest-tickets.component';
import { BuySuccessComponent } from './components/buy-success/buy-success.component';
import { PolicyPanelComponent } from './components/shared/policy-panel/policy-panel.component';
import { TermsAndConditionsComponent } from './components/terms-and-conditions/terms-and-conditions.component';
import { DataPolicyComponent } from './components/data-policy/data-policy.component';
import { ViewPaymentDialogComponent } from './dialogs/view-payment-dialog/view-payment-dialog.component';
import { FeatureDisabledPanelComponent } from './components/shared/feature-disabled-panel/feature-disabled-panel.component';
import { PolicyPanelNoPurchasesComponent } from './components/shared/policy-panel-no-purchases/policy-panel-no-purchases.component';
import { BuyPondGateKeysComponent } from './components/buy-pond-gate-keys/buy-pond-gate-keys.component';
import { JuniorProgrammeIntroComponent } from './components/junior-programme-intro/junior-programme-intro.component';
import { JuniorTrophiesComponent } from './components/junior-trophies/junior-trophies.component';
import { JuniorTacklePleaComponent } from './components/junior-tackle-plea/junior-tackle-plea.component';
import { JuniorMatchesComponent } from './components/junior-matches/junior-matches.component';
import { JuniorSponsorsComponent } from './components/junior-sponsors/junior-sponsors.component';

import { MatTableExporterModule } from 'mat-table-exporter';
import { CalendarExportDialogComponent } from './dialogs/calendar-export-dialog/calendar-export-dialog.component';
import { TrophyWinnersComponent } from './components/trophy-winners/trophy-winners.component';
import { RegisterComponent } from './components/register/register.component';
import { JuniorOpenRegistrationsComponent } from './components/junior-open-registrations/junior-open-registrations.component';
import { RegistrationsComponent } from './components/registrations/registrations.component';
import { RegistrationPanelComponent } from './components/registration-panel/registration-panel.component';
import { GenerateBookPrintingPagesComponent } from './components/generate-book-printing-pages/generate-book-printing-pages.component';
import { ConfirmKeyDialogComponent } from './dialogs/confirm-key-dialog/confirm-key-dialog.component';
import { MatchBookedComponent } from './dialogs/match-booked/match-booked.component';

@NgModule({
  declarations: [
    AppComponent,
    NavComponent,
    WelcomeComponent,
    NewsComponent,
    WatersComponent,
    MatchesComponent,
    PhotosComponent,
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
    AddEditWaterDialogComponent,
    AddEditRulesDialogComponent,
    ClubOfficersComponent,
    FormsComponent,
    AnglingTrustComponent,
    EnvironmentalComponent,
    AddMemberComponent,
    RulesGeneralComponent,
    RulesMatchComponent,
    RulesJnrGeneralComponent,
    RulesJnrMatchComponent,
    GuestTicketCreateComponent,
    GuestTicketsComponent,
    CreateGuestTicketComponent,
    BuyMembershipsComponent,
    PaymentsComponent,
    BuyDayTicketsComponent,
    BuyGuestTicketsComponent,
    BuySuccessComponent,
    PolicyPanelComponent,
    TermsAndConditionsComponent,
    DataPolicyComponent,
    ViewPaymentDialogComponent,
    FeatureDisabledPanelComponent,
    PolicyPanelNoPurchasesComponent,
    BuyPondGateKeysComponent,
    JuniorProgrammeIntroComponent,
    JuniorTrophiesComponent,
    JuniorTacklePleaComponent,
    JuniorMatchesComponent,
    JuniorSponsorsComponent,
    CalendarExportDialogComponent,
    TrophyWinnersComponent,
    RegistrationsComponent,
    RegisterComponent,
    JuniorOpenRegistrationsComponent,
    RegistrationsComponent,
    RegistrationPanelComponent,
    GenerateBookPrintingPagesComponent,
    ConfirmKeyDialogComponent,
    MatchBookedComponent,
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
    MatSelectModule,
    MatAutocompleteModule,
    MatBadgeModule,
    ScrollingModule, 
    FateModule,
    FateMaterialModule,
    MatTableExporterModule,
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorIntercept,
      multi: true,
      deps: [MatDialog, AuthenticationService, Router]
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
  bootstrap: [AppComponent],
  schemas: [ CUSTOM_ELEMENTS_SCHEMA ]
})
export class AppModule { }
