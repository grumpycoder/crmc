//manager.js
//mark.lawrence

(function () {
    'use strict';

    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, CensorController);

    CensorController.$inject = ['logger', '$uibModal', 'censorService'];

    function CensorController(logger, $modal, service) {
        var vm = this;
        vm.title = 'Censor Manager';
        vm.description = 'View and edit censored words';
        vm.isBusy = false;

        vm.censors = [];
        vm.currentEdit = {};
        vm.lastDeleted = null;
        vm.lastUpdated = null;
        vm.itemToEdit = {};

        var tableStateRef;

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
        };

        vm.search = function (tableState) {
            tableStateRef = tableState;

            var searchTerm = '';
            if (typeof (tableState.search.predicateObject) != "undefined") {
                searchTerm = tableState.search.predicateObject.searchTerm;
            }
            vm.isBusy = true;
            service.query(searchTerm)
                .then(function (data) {
                    vm.censors = data;
                    vm.isBusy = false;
                });
        };

        vm.cancelEdit = function (id) {
            vm.currentEdit[id] = false;
        };

        vm.create = function () {
            var item = {};
            $modal.open({
                templateUrl: '/app/censors/views/censor.html',
                controller: ['$uibModalInstance', 'censorService', 'item', createCensorController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            })
                .result.then(function (data) {
                    vm.censors.unshift(data);
                });
        };

        vm.deleteItem = function (censor) {
            vm.lastDeleted = censor;
            service.remove(censor.id)
                .then(function (data) {
                    var idx = vm.censors.indexOf(censor);
                    vm.censors.splice(idx, 1);
                });
        };

        vm.editItem = function (censor) {
            vm.currentEdit[censor.id] = true;
            vm.itemToEdit = angular.copy(censor);
        };

        vm.saveItem = function (censor) {
            vm.currentEdit[censor.id] = false;
            angular.copy(censor, vm.lastUpdated = {});
            angular.extend(censor, vm.itemToEdit);
            service.update(vm.itemToEdit)
                .then(function (data) {
                    censor = data;
                });
        };

        vm.undoDelete = function () {
            service.create(vm.lastDeleted)
                .then(function (data) {
                    logger.success('Successfully restored ' + data.word);
                    vm.censors.unshift(data);
                    vm.lastDeleted = null;
                });
        };

        vm.undoChange = function () {
            service.update(vm.lastUpdated)
                .then(function (data) {
                    angular.forEach(vm.censors,
                        function (u, i) {
                            if (u.id === vm.lastUpdated.id) {
                                vm.censors[i] = vm.lastUpdated;
                            }
                        });
                    logger.success('Successfully restored ' + data.word);
                    vm.lastUpdated = null;
                });
        };
    };

    function createCensorController($modal, service, item) {
        var vm = this;

        vm.item = angular.copy(item);

        vm.close = close;
        vm.save = save;

        function close() {
            $modal.dismiss();
        };

        function save() {
            service.create(vm.item)
                .then(function (data) {
                    vm.item = data;
                    $modal.close(vm.item);
                });
        };
    };
})();