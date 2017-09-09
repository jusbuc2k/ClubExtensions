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

    async previewClicked() {
        var result = await this.http.fetch("/api/PreviewClubAssignments", {
            credentials: 'same-origin',
            method: "post",
        });

        var preview = await result.json();

        this.previewList = preview.list;
        this.changeCount = preview.changeCount;
    }

    cancelClicked() {
        this.previewList = null;
        this.changeCount = 0;
    }

    async applyChangesClicked() {
        var result = await this.http.fetch("/api/AssignClubs", {
            credentials: 'same-origin',
            method: "post",
            body: json(this.previewList.filter(x => x.isChange))
        });

        if (!result.ok) {
            this.errorMessage = `Failed to apply changes: ${result.statusText}`;
        }

        this.success = true;
        this.changeCount = 0;
        this.previewList = null;
    }


}
