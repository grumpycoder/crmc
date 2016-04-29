//app.module.js
//mark.lawrence

(function () {
    angular.module('app', [
        //application modules
        'app.core',
        'app.service',
        'app.filter',

        //features
        'app.home',
        'app.censors',
        'app.people',
        'app.settings',
        'app.users'
    ]);
})()