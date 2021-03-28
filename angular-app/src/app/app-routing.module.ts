import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { WelcomeComponent } from './components/welcome/welcome.component';
import { NewsComponent } from './components/news/news.component';
import { WatersComponent } from './components/waters/waters.component';
import { MatchesComponent } from './components/matches/matches.component';
import { PhotosComponent } from './components/photos/photos.component';
import { RulesComponent } from './components/rules/rules.component';

const routes: Routes = [
  { path: '', redirectTo: '/welcome', pathMatch: 'full'},
  { path: 'welcome', component: WelcomeComponent },
  { path: 'news', component: NewsComponent },
  { path: 'waters', component: WatersComponent },
  { path: 'matches', component: MatchesComponent },
  { path: 'photos', component: PhotosComponent },
  { path: 'rules', component: RulesComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
