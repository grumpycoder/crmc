(function () {
    'use strict';

    var controllerId = 'HomeController';

    angular.module('app.home').controller(controllerId, mainController);

    mainController.$inject = ['$log', 'peopleService', 'censorService', 'storage', 'config'];

    function mainController(log, peopleService, censorService, storage, config) {
        var vm = this;
        vm.title = 'Home';

        vm.stat = {};
        vm.people = [];

        activate();

        var censors = [];

        function activate() {
            log.info(controllerId + ' active');
            log.info(config);
            log.info(config.apiEndPoints);
            censors = JSON.parse(storage.get('censors'));
            if (!censors) {
                censorService.query('')
                        .then(function (data) {
                            storage.set('censors', JSON.stringify(data));
                            censors = JSON.parse(storage.get('censors'));
                        });
            }

            refresh();
        }

        function refresh() {
            peopleService.getCurrentStats()
                .then(function (data) {
                    vm.stat = data;
                });

            peopleService.get()
                .then(function (data) {
                    vm.people = data;
                });
        }
    }
})();