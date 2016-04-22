//manager.js
//mark.lawrence

(function () {
    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, ['$log', 'censorService', mainController]);

    function mainController(log, service) {
        var vm = this;

        vm.censors = [];

        activate();

        function activate() {
            log.info('Censor Controller');
            loadCensors();
        }

        function loadCensors() {
            vm.censors = [
                { word: 'Name1' },
                { word: 'Name2' }
            ];
        }
    }
})()