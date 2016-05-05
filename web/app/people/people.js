//manager.js
//mark.lawrence

(function () {
    var controllerId = 'PeopleController';

    angular.module('app.people').controller(controllerId, mainController);

    mainController.$inject = ['$log', 'peopleService', '$uibModal'];

    function mainController(logger, service, $modal) {
        var vm = this;
        vm.title = "People";

        vm.addItem = addItem;
        vm.deleteItem = deleteItem;
        vm.editItem = editItem;
        vm.quickFilter = quickFilter;
        vm.isLocal = null;
        vm.daysFilter = '0';

        vm.people = [];
        vm.paged = paged;
        vm.search = search;
        vm.searchTerm = '';

        vm.searchModel = {
            page: 1,
            pageSize: 15
        };

        var tableStateRef;

        activate();

        function activate() {
            logger.log(controllerId + ' activated');
        }

        function addItem() {
            var item = {
                fuzzyMatchValue: 0.0
            };
            $modal.open({
                templateUrl: '/app/people/views/person.html',
                controller: ['$log', '$uibModalInstance', 'peopleService', 'item', 'storage', editPersonController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            }).result.then(function (data) {
                vm.people.unshift(data);
            });
        }

        function editItem(item) {
            $modal.open({
                templateUrl: '/app/people/views/person.html',
                controller: ['$log', '$uibModalInstance', 'peopleService', 'item', 'storage', editPersonController],
                controllerAs: 'vm',
                resolve: {
                    item: function () { return item; }
                }
            });
        }

        function deleteItem(person) {
            service.remove(person.id).then(function (data) {
                var idx = vm.people.indexOf(person);
                vm.people.splice(idx, 1);
            });
        }

        function search(tableState) {
            tableStateRef = tableState;

            if (vm.daysFilter !== '0') vm.searchModel.dateCreated = moment().subtract(parseInt(vm.daysFilter), 'days').format('MM/DD/YYYY');
            if (vm.daysFilter === '0') vm.searchModel.dateCreated = null;

            if (vm.highMatch) vm.searchModel.fuzzyMatchValue = 0.8;

            if (vm.medMatch) vm.searchModel.fuzzyMatchValue = 0.5;

            if (!vm.highMatch && !vm.medMatch) vm.searchModel.fuzzyMatchValue = null;

            if (vm.isLocal) vm.searchModel.isDonor = false;
            if (!vm.isLocal) vm.searchModel.isDonor = null;

            if (!vm.searchModel.isPriority) vm.searchModel.isPriority = null;

            //TODO: Sort not implemented
            if (typeof (tableState.sort.predicate) != "undefined") {
                vm.searchModel.orderBy = tableState.sort.predicate;
                vm.searchModel.orderDirection = tableState.sort.reverse ? 'desc' : 'asc';
            }
            if (typeof (tableState.search.predicateObject) != "undefined") {
                vm.searchModel.firstname = tableState.search.predicateObject.firstname;
                vm.searchModel.lastname = tableState.search.predicateObject.lastname;
                vm.searchModel.zipcode = tableState.search.predicateObject.zipCode;
                vm.searchModel.emailAddress = tableState.search.predicateObject.emailAddress;
                vm.searchModel.isDonor = tableState.search.predicateObject.isDonor;
                vm.searchModel.isPriority = tableState.search.predicateObject.isPriority;
                vm.searchModel.dateCreated = tableState.search.predicateObject.dateCreated;
                vm.searchModel.fuzzyMatchValue = tableState.search.predicateObject.fuzzyMatchValue;
            }

            service.query(vm.searchModel).then(function (data) {
                vm.people = data.items;
                vm.searchModel = data;
            });
        }

        function paged(pageNum) {
            search(tableStateRef);
        }

        function quickFilter() {
            search(tableStateRef);
        }
    }

    function editPersonController(log, $modal, service, item, storage) {
        var vm = this;

        //TODO: Can be made global??
        var censors = JSON.parse(storage.get('censors'));

        vm.item = angular.copy(item);

        vm.close = close;
        vm.save = save;
        vm.matchValue = matchValue;

        function close() {
            $modal.dismiss();
        }

        function matchValue() {
            var value = 0.0;
            var fn = vm.item.firstname + ' ' + vm.item.lastname;
            _.forEach(censors, function (censor) {
                var idx = clj_fuzzy.metrics.dice(fn, censor.word);
                if (idx > value) value = idx;
            });
            vm.item.fuzzyMatchValue = value;
        }

        function save() {
            if (vm.item.id) {
                service.update(vm.item).then(function () {
                    angular.extend(item, vm.item);
                    $modal.close(vm.item);
                });
            } else {
                service.create(vm.item).then(function (data) {
                    $modal.close(data);
                });
            }
        }
    }
})()