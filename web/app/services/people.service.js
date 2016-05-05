﻿//people.service.js
//mark.lawrence

(function () {
    var serviceId = 'peopleService';
    angular.module('app.service').factory(serviceId, serviceController);

    serviceController.$inject = ['$log', '$http', 'config'];

    function serviceController(log, $http, config) {
        log.info(serviceId + ' loaded');
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
            return $http.put(url, censor)
                .then(function (response) {
                    return response.data;
                });
        }

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function query(search) {
            return $http.post(url + '/search', search)
                .then(function (response) {
                    return response.data;
                });
        }

        function update(person) {
            return $http.put(url, person)
                .then(function (response) {
                    return response.data;
                });
        }

        function remove(id) {
            return $http.delete(url + id)
                .then(function (response) {
                    return response.data;
                });
        }

        function getCurrentStats() {
            return $http.get(url + '/stat')
                .then(function (response) {
                    return response.data;
                });
        }
    }
})()