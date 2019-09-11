import { HttpClient } from "aurelia-fetch-client";
import { inject } from "aurelia-framework";

@inject(HttpClient)
export class Reports {

    constructor(protected http: HttpClient) {

    }

    async clearCache() {
        try {
            await this.http.fetch("/api/Cache/Reset", {
                credentials: 'same-origin',
                method: "post",
            });

            this.cacheClearSuccess = true;
        }
        catch (e) {
            alert("Error clearing cache");
        }
    }

    cacheClearSuccess: boolean = false;
}
