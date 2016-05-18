//mark.lawrence
//kiosk.js

(function () {
    var controllerId = 'KioskController';
    angular.module('app').controller(controllerId, mainController);

    mainController.$inject = ['$scope', '$log', '$state', '$timeout', 'peopleService', 'censors'];

    function mainController($scope, logger, $state, $timeout, peopleService, censors) {
        var vm = this;
        var tableStateRef;
        var prevSelection = null;
        var timer;
        var waitTime = 5000;

        vm.title = 'welcome';
        vm.people = [];

        vm.searchModel = {
            page: 1,
            pageSize: 13
        };
        vm.showValidationErrors = false;

        vm.toggleName = function (person) {
            if (prevSelection) {
                prevSelection.$selected = false;
            }
            if (prevSelection === person) {
                person.$selected = false;
                vm.person = undefined;
                prevSelection = null;
            } else {
                person.$selected = true;
                vm.person = person;
                prevSelection = person;
            }
            resetTimer();
        }

        vm.setFocus = function (event) {
            vm.editItem = event;

            if (event) vm.editItem = event;
        }

        vm.keyboardInput = function (key) {
            resetTimer();
            vm.showValidationErrors = true;
            var keyCode = key.currentTarget.outerText;
            if (keyCode === 'SPACE') {
                vm.lastLetterIsSpace = true;
                return;
            }

            if (keyCode === 'DEL') {
                vm.editItem.$setViewValue(vm.editItem.$viewValue.substr(0, vm.editItem.$viewValue.length - 1));
                vm.editItem.$render();
                return;
            }

            if (vm.lastLetterIsSpace) {
                vm.editItem.$setViewValue(vm.editItem.$viewValue + ' ' + keyCode);
                vm.lastLetterIsSpace = false;
            } else {
                vm.editItem.$setViewValue(vm.editItem.$viewValue + keyCode);
            }
            vm.editItem.$render();
        }

        vm.paged = function () {
            prevSelection = null;
            vm.search(tableStateRef);
        }

        vm.search = function (tableState) {
            tableStateRef = tableState;

            if (vm.searchForm !== undefined && vm.searchForm.$invalid) {
                vm.showValidationErrors = true;
                return;
            }
            var names = vm.searchTerm.split(' ');
            var fn = names[1] ? names[0] : '';
            var ln = names[1] || names[0];

            vm.searchModel.firstname = fn;
            vm.searchModel.lastname = ln;

            peopleService.query(vm.searchModel).then(function (data) {
                vm.people = data.items;
                vm.searchModel = data;

                $state.go('home.list').then(function () {
                    resetTimer();
                });
            });
        }

        vm.goBack = function () {
            resetTimer();
            history.back();
        }

        vm.gotoPledge = function () {
            if (vm.createForm.$invalid) {
                vm.showValidationErrors = true;
                toastr.error('Please correct your information');
                return;
            }

            vm.person.dateCreated = moment().format('MM/DD/YYYY HH:mm:ss');
            vm.person.isDonor = vm.person.isPriority = false;
            vm.person.firstname = Humanize.titleCase(vm.person.firstname.toLowerCase());
            vm.person.lastname = Humanize.titleCase(vm.person.lastname.toLowerCase());
            vm.person.fuzzyMatchValue = matchValue(vm.person);
            if (vm.person.emailAddress) vm.person.emailAddress = vm.person.emailAddress.toLowerCase();

            $state.go('home.pledge').then(function () {
                resetTimer();
            });
        }

        vm.gotoSearch = function () {
            $state.go('home.search').then(function () {
                vm.showValidationErrors = false;
                vm.editItem = vm.searchForm.searchTerm;
                resetTimer();
            });
        }

        vm.gotoCreate = function gotoCreate() {
            $state.go('home.create').then(function () {
                vm.editItem = vm.createForm.firstname;
                vm.showValidationErrors = false;
                vm.person = { firstname: '', lastname: '' };
                createValidationWatcher();
                resetTimer();
            });
        }

        vm.cancel = function () {
            vm.person = { firstname: '', lastname: '' };
            vm.searchTerm = '';
            vm.people = [];

            $state.go('home').then(function () {
                $timeout.cancel(timer);
            });
        }

        vm.finish = function () {
            $state.go('home.finish').then(function () {
                logger.info('sending to hub');
                //TODO: Send to hub service
                logger.info(vm.person);
                resetTimer();
            });
        }

        vm.save = function () {
            peopleService.create(vm.person).then(function (data) {
                vm.finish();
            });
        }

        function matchValue(person) {
            var value = 0.0;
            var fn = getFullName(person);

            _.forEach(censors, function (censor) {
                var idx = clj_fuzzy.metrics.dice(fn, censor.word);
                if (idx > value) value = idx;
            });
            return value;
        }

        function getFullName(person) {
            return person.firstname + ' ' + person.lastname;
        }

        function resetTimer() {
            $timeout.cancel(timer);
            timer = $timeout(function () {
                vm.cancel();
            }, waitTime);
        }

        function createValidationWatcher() {
            //logger.info('creating watcher', vm.person, false);
            $scope.$watchCollection('vm.person', function (newVal, oldVal) {
                if (vm.person) {
                    resetTimer();
                    var valid = validatePerson(vm.person);
                    if (vm.createForm) {
                        vm.createForm.firstname.$setValidity('blacklist', valid);
                        vm.createForm.lastname.$setValidity('blacklist', valid);
                    }
                }
            });
        }

        function validatePerson(person) {
            var fullName = (person.firstname ? person.firstname : '') + ' ' + (person.lastname ? person.lastname : '');
            var firstmatch = censors.some(function (e) { return e.word.toUpperCase() === (person.firstname ? person.firstname.toUpperCase() : ''); });
            var lastmatch = censors.some(function (e) { return e.word.toUpperCase() === (person.lastname ? person.lastname.toUpperCase() : ''); });
            var fullmatch = censors.some(function (e) { return e.word.toUpperCase() === fullName.toUpperCase(); });

            var match = firstmatch || lastmatch || fullmatch;
            return !match;
        }
    }
})();