//manager.js
//mark.lawrence

(function () {
    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, ['$log', 'censorService', mainController]);

    function mainController(log, service) {
        var vm = this;

        vm.censors = [];
        vm.searchTerm = '';
        vm.search = search;

        activate();

        function activate() {
            log.info('Censor Controller');
            search();
        }

        //function getCensors() {
        //    service.query(vm.searchTerm)
        //        .then(function (data) {
        //            vm.censors = data;
        //        });
        //    //vm.censors = [
        //    //    { word: 'Name1' },
        //    //    { word: 'Name2' }
        //    //];
        //}

        function search() {
            log.info(vm.searchTerm);
            service.query(vm.searchTerm).then(function (data) {
                vm.censors = data;
            });
            log.info(vm.censorName);
        }
    }
})()