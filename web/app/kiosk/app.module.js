(function () {
    "use strict";
    var app = angular.module('app',
    [
        //Angular Modules
        'ui.router',
        'ngMessages',

        //application modules
        'app.core',
        'app.service',

        //3rd Party Modules
    ]);

    app.config(['$stateProvider', '$urlRouterProvider', config]);

    function config($stateProvider, $urlRouterProvider) {
        //$locationProvider.html5Mode(true);

        $stateProvider
            .state('home',
                    {
                        url: '/',
                        templateUrl: 'app/kiosk/views/home.html',
                        controller: 'KioskController',
                        controllerAs: 'vm',
                        resolve: {
                            censors: function (censorService) {
                                return censorService.get();
                            }
                        }
                    })
            .state('home.create',
                    {
                        url: 'create',
                        templateUrl: 'app/kiosk/views/partial-create.html'
                    })
            .state('home.search',
                    {
                        url: 'search',
                        templateUrl: 'app/kiosk/views/partial-search.html'
                    })
            .state('home.pledge',
                    {
                        url: 'pledge',
                        templateUrl: 'app/kiosk/views/partial-pledge.html'
                    })
            .state('home.list',
                    {
                        url: 'list',
                        templateUrl: 'app/kiosk/views/partial-list.html'
                    })
            .state('home.finish',
                    {
                        url: 'finish',
                        templateUrl: 'app/kiosk/views/partial-finish.html'
                    })
        ;

        $urlRouterProvider.otherwise('/');
    }
}());