//people.service.js
//mark.lawrence

(function () {
    'use strict';

    var serviceId = 'peopleService';
    angular.module('app.service').factory(serviceId, serviceController);

    function serviceController(logger, $http, config, exception) {
        logger.log(serviceId + ' loaded');
        var url = config.apiUrl + config.apiEndPoints.Person;

        var service = {
            create: create,
            get: get,
            query: query,
            update: update,
            remove: remove,
            getCurrentStats: getCurrentStats
        }

        return service;

        function create(censor) {
            return $http.post(url, censor).then(_success);
        }

        function get() {
            return $http.get(url)
                .then(_success)
                .catch(function (message) { exception.catcher('Failed to retrieve people')(message); });
        }

        function query(search) {
            return $http.post(url + '/search', search)
                .then(_success)
                .catch(function (message) { exception.catcher('Failed to retrieve people')(message); });
        }

        function update(person) {
            return $http.put(url, person).then(_success);
        }

        function remove(id) {
            return $http.delete(url + '/' + id).then(_success);
        }

        function getCurrentStats() {
            return $http.get(url + '/stat')
                .then(_success)
                .catch(function (message) { exception.catcher('Failed to retrieve stats')(message); });
        }

        function _success(response) {
            return response.data;
        }
    }
})();