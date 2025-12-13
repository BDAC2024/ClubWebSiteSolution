export class DisplayedColumnsForEvents {
    day: [on: boolean, order: number] = [true, 0];
    date: [on: boolean, order: number] = [true, 1];
    time: [on: boolean, order: number] = [true, 2];
    description: [on: boolean, order: number] = [true, 3];
    cup: [on: boolean, order: number] = [true, 4];
    more: [on: boolean, order: number] = [true, 5];

    public get displayedColumns(): string[] {
        var colsToDisplay: string[] = [];

        if (this.day[0] == true) { colsToDisplay[this.day[1]] = "day"}
        if (this.date[0] == true) { colsToDisplay[this.date[1]] = "date"}
        if (this.time[0]) { colsToDisplay[this.time[1]] = "time"}
        if (this.description[0]) { colsToDisplay[this.description[1]] = "description"}
        if (this.cup[0]) { colsToDisplay[this.cup[1]] = "cup"}
        if (this.more[0]) { colsToDisplay[this.more[1]] = "more"}

        var sortedColsToDisplay: string[] = [];
        
        colsToDisplay.forEach((col) => {
            if (col != "") { sortedColsToDisplay.push(col); }
        });

        return sortedColsToDisplay;
    }

    public SetAll(): void {
        this.day[0] = true;
        this.date[0] = true;
        this.time[0] = true;
        this.description[0] = true;
        this.cup[0] = true;
        this.more[0] = true;
    }

    public ClearAll(): void {
        this.day[0] = false;
        this.date[0] = false;
        this.time[0] = false;
        this.description[0] = false;
        this.cup[0] = false;
        this.more[0] = false;
    }
   
}