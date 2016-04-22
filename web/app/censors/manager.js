//manager.js
//mark.lawrence

(function () {
    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, ['$log', MainController]);

    function MainController(log) {
        var vm = this;

        log.info('Censor Controller');
    }
})()