import { HttpClient } from 'aurelia-fetch-client';
import { inject } from 'aurelia-framework';

@inject(HttpClient)
export class Fetchdata {
    constructor(protected http: HttpClient) {

    }

    public clubbers: any[];

    async assignClubsClicked() {
        var result = await this.http.fetch("/api/AssignClubs", {
            credentials: 'same-origin',
            method: "post",
        });

        this.clubbers = await result.json();
    }


}
