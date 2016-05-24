//manager.js
//mark.lawrence

(function () {
    var controllerId = 'CensorController';

    angular.module('app.censors').controller(controllerId, mainController);

    mainController.$inject = ['logger', '$uibModal', 'censorService'];

    function mainController(logger, $modal, service) {
        var vm = this;
        vm.title = 'Censor Manager';
        vm.description = 'View and edit censored words';
        vm.cancelEdit = cancelEdit;
        vm.create = create;
        vm.deleteItem = deleteItem;
        vm.editItem = editItem;
        vm.isBusy = false;
        vm.saveItem = saveItem;

        vm.censors = [];
        vm.currentEdit = {};
        vm.search = search;

        var tableStateRef;

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
        }

        function search(tableState) {
            tableStateRef = tableState;

            var searchTerm = '';
            if (typeof (tableState.search.predicateObject) != "undefined") {
                searchTerm = tableState.search.predicateObject.searchTerm;
            }
            vm.isBusy = true;
            service.query(searchTerm).then(function (data) {
                vm.censors = data;
                vm.isBusy = false;
            });
        }

        function cancelEdit(id) {
            vm.currentEdit[id] = false;
        }

        function create() {
            var item = {};
            $modal.open({
                templateUrl: '/app/censors/views/censor.html',
                controller: ['$uibModalInstance', 'censorService', 'item', createCensorController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            }).result.then(function (data) {
                vm.censors.unshift(data);
            });
        }

        function deleteItem(censor) {
            service.remove(censor.id)
                .then(function (data) {
                    var idx = vm.censors.indexOf(censor);
                    vm.censors.splice(idx, 1);
                });
        }

        function editItem(censor) {
            vm.currentEdit[censor.id] = true;
            vm.itemToEdit = angular.copy(censor);
        }

        function saveItem(censor) {
            vm.currentEdit[censor.id] = false;
            service.update(censor).then(function (data) {
                censor = data;
            });
        }
    }

    function createCensorController($modal, service, item) {
        var vm = this;

        vm.item = angular.copy(item);

        vm.close = close;
        vm.save = save;

        function close() {
            $modal.dismiss();
        }

        function save() {
            service.create(vm.item).then(function (data) {
                vm.item = data;
                $modal.close(vm.item);
            });
        }
    }
})()