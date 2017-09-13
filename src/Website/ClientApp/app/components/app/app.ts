import { Aurelia, PLATFORM } from 'aurelia-framework';
import { Router, RouterConfiguration } from 'aurelia-router';
import * as $ from "jquery";

export class App {
    router: Router;

    configureRouter(config: RouterConfiguration, router: Router) {
        config.title = 'Club Extensions for PCO';

        config.map([{
            route: [ '', 'home' ],
            name: 'home',
            settings: { icon: 'home' },
            moduleId: PLATFORM.moduleName('../home/home'),
            nav: true,
            title: 'Home'
        },
        {
            route: 'forms',
            name: 'forms',
            settings: { icon: 'file' },
            moduleId: PLATFORM.moduleName('../reports/reports'),
            nav: true,
            title: 'Forms & Reports'
        },
        {
            route: 'counter',
            name: 'counter',
            settings: { icon: 'education' },
            moduleId: PLATFORM.moduleName('../counter/counter'),
            nav: false,
            title: 'Counter'
        }, {
            route: 'assign-clubs',
            name: 'assign-clubs',
            settings: { icon: 'th-list' },
            moduleId: PLATFORM.moduleName('../tools/AssignClubs'),
            nav: true,
            title: 'Assign Clubs'
        }]);

        router["username"] = window["$Environment"].username;

        this.router = router;
    }

    activate() {
        $(document).on('click', '.navbar-collapse.in', function (e) {
            if ($(e.target).is('a') && $(e.target).attr('class') != 'dropdown-toggle') {
                $(this).collapse('hide');
            }
        }); 
    }
}
