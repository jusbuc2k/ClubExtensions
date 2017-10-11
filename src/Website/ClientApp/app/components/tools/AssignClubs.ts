import { HttpClient, json } from 'aurelia-fetch-client';
import { inject } from 'aurelia-framework';

@inject(HttpClient)
export class AssignClubs {
    constructor(protected http: HttpClient) {

    }

    public previewList: any[];
    public changeCount: number = 0;
    public success: boolean = false;
    public errorMessage: string;
    public isWorking: boolean = false;

    async previewClicked() {
        this.isWorking = true;
        this.errorMessage = null;

        var result = await this.http.fetch("/api/PreviewClubAssignments", {
            credentials: 'same-origin',
            method: "post",
        });

        var preview = await result.json();

        this.previewList = preview.list.map(clubber => {
            return Object.assign(clubber, {
                applyChange: !clubber.oldClubName
            });
        });

        this.isWorking = false;

        this.changeCount = preview.changeCount;
    }

    checkAll() {
        this.previewList.forEach(item => {
            item.applyChange = item.isChange;
        });
    }

    cancelClicked() {
        this.previewList = null;
        this.errorMessage = null;
        this.changeCount = 0;
    }

    async applyChangesClicked() {
        let changes = this.previewList.filter(x => x.isChange && x.applyChange);

        if (changes.length <= 0) {
            this.errorMessage = "No changes selected to apply.";
            return;
        }
        
        var result = await this.http.fetch("/api/AssignClubs", {
            credentials: 'same-origin',
            method: "post",
            body: json(changes)
        });

        if (!result.ok) {
            this.errorMessage = `Failed to apply changes: ${result.statusText}`;
        }

        this.success = true;
        this.changeCount = 0;
        this.previewList = null;
    }


}
