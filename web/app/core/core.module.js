//core.module.js
//mark.lawrence

(function () {
    angular.module('app.core', [
        //angular modules
        'ngMessages',
        'angularLocalStorage',

        //custom modules
        'blocks.logger',
        'blocks.exception',

        //third party modules
        'smart-table',
        'ui.bootstrap',
        'rzModule',
        'ngTagsInput',
        'switcher',
        'nzToggle',
    ]);
})()