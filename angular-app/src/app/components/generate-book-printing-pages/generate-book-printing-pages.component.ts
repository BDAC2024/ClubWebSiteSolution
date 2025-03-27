import { Component, OnInit } from '@angular/core';
import { List } from 'postcss/lib/list';
import { PrintPaginationSummary } from 'src/app/models/print-pagination-summary';
import { UtilityService } from 'src/app/services/utility.service';

@Component({
  selector: 'app-generate-book-printing-pages',
  templateUrl: './generate-book-printing-pages.component.html',
  styleUrls: ['./generate-book-printing-pages.component.css']
})
export class GenerateBookPrintingPagesComponent implements OnInit {

  public numPages!: number;
  public printCoverSeparately!: boolean;
  public printCoverSeparatelyAsString: string = "";

  public errorMessage: string = "";
  public isLoading: boolean = false;

  public printPaginationSummary!: PrintPaginationSummary[];
  public results: string = "";

  constructor(private utilityService: UtilityService) { }

  ngOnInit(): void {
  }

  public calcPages(): void{
    this.errorMessage = "";
    this.results = "";

    if (!this.numPages || !this.printCoverSeparately)
    {
      this.errorMessage = "Please answer both questions!";
    }
    else
    {
      console.log(`numPages: ${this.numPages}`)
      console.log(`printCoverSeparately: ${this.printCoverSeparately}`)

      this.printCoverSeparatelyAsString = this.printCoverSeparately.toString();
      this.isLoading = true;
      this.printPaginationSummary = [];
      
      this.utilityService.getBookPrintingPages(this.numPages, this.printCoverSeparately)
      .subscribe(data => {
        this.isLoading = false;
        this.printPaginationSummary = data;

        this.results = "<h2>Results</h2>";

        this.results += "In the Print dialogue: -<br/>"
        this.results += this.stdOpeningText();

        // this.results += `this.printCoverSeparately ${this.printCoverSeparately}<br/>`
        // if (this.printCoverSeparatelyAsString == "true")
        // {
        //   this.results += `IT IS TRUE<br/>`
        // }
        // else
        // {
        //   this.results += `IT ISNT TRUE<br/>`
        // }
        // if (this.printCoverSeparatelyAsString == "false")
        // {
        //   this.results += `IT ISNT TRUE<br/>`
        // }
        // else
        // {
        //   this.results += `IT IS TRUE<br/>`
        // }
    
        if (this.printCoverSeparatelyAsString == "true")
        {
          if (this.printPaginationSummary[0].instructions != "")
          {
              this.results += ` - ${this.printPaginationSummary[0].instructions}<br/>`;
          }
          this.results += `   <b>${this.printPaginationSummary[0].pagesToPrint}</b><br/><br/>`;

          this.results += "Cut the pages in the middle horizontally.<br/><br/>";

          this.results += "For the inside pages; In the Print dialogue: -<br/>";
          this.results += this.stdOpeningText();

          if (this.printPaginationSummary[1].instructions != "")
          {
            this.results += ` - ${this.printPaginationSummary[1].instructions}<br/>`;
          }
          this.results += `   <b>${this.printPaginationSummary[1].pagesToPrint}</b><br/><br/>`;
        }
        else
        {
          this.printPaginationSummary.forEach(element => {
            if (element.instructions != "")
            {
              this.results += element.instructions + "<br/><br/>";
            }
            this.results += "<b>" + element.pagesToPrint + "</b><br/><br/>";
          });
        }
        this.results += this.stdClosingText();
      });
  
    }
  }

  private stdOpeningText(): string
  {
    return " - Choose '4 pages per sheet'<br/>" + 
           " - Choose 'Portrait Orientation'<br/><br/>";
  }

  private stdClosingText(): string
  {
    return "Cut the pages in the middle horizontally to create a top and bottom pile.<br/>" + 
            "Place one pile on top of the other pile then fold!"
  }
}
