import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { IGrid } from './grid-model';
import { GridStatusService } from './grid-status.service';
import { Observable } from 'rxjs';


@Component({
  selector: 'grid-status',
  templateUrl: './grid-status.component.html',
  providers: [GridStatusService]
})
export class GridStatusComponent {
  public grids: IGrid[];


  constructor(
    private gridStatusService: GridStatusService
  ) { }

  ngOnInit() {
    this.grids = this.gridStatusService.getGrids();
  }


  public getGrids() {
    this.gridStatusService.getGrids();
  }

  public toggleGridActivation(grid: IGrid) {
    console.log(`Grid is ${grid}`)
    this.gridStatusService.toggleGridActivation(grid);
  }
}

