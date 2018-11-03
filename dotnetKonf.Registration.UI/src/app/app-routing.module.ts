import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LicenceComponent } from './licence/licence.component';
const routes: Routes = [
  {path:'', component: HomeComponent},
  {path:'licence', component: LicenceComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
