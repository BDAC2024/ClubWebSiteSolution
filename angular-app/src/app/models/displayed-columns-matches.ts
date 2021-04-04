export class DisplayedColumnsForMatches {
    date: [on: boolean, order: number] = [true, 0];
    day: [on: boolean, order: number] = [true, 1];
    description: [on: boolean, order: number] = [true, 2];
    cup: [on: boolean, order: number] = [true, 3];
    number: [on: boolean, order: number] = [true, 4];
    more: [on: boolean, order: number] = [true, 5];

    public get displayedColumns(): string[] {
        var colsToDisplay: string[] = [];

        if (this.date[0] == true) { colsToDisplay[this.date[1]] = "date"}
        if (this.day[0] == true) { colsToDisplay[this.day[1]] = "day"}
        if (this.description[0]) { colsToDisplay[this.description[1]] = "description"}
        if (this.cup[0]) { colsToDisplay[this.cup[1]] = "cup"}
        if (this.number[0]) { colsToDisplay[this.number[1]] = "number"}
        if (this.more[0]) { colsToDisplay[this.more[1]] = "more"}

        var sortedColsToDisplay: string[] = [];
        
        colsToDisplay.forEach((col) => {
            if (col != "") { sortedColsToDisplay.push(col); }
        });

        return sortedColsToDisplay;
    }
}