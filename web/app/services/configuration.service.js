//configuration.service.js
//mark.lawrence

(function () {
    var serviceId = 'configurationService';
    angular.module('app.service').factory(serviceId, serviceController);

    serviceController.$inject = ['$log', '$http', 'config'];

    function serviceController(log, $http, config) {
        log.info(serviceId + ' loaded ');
        var url = config.apiUrl + config.apiEndPoints.Configuration;

        var service = {
            get: get,
            update: update
        }

        return service;

        function get() {
            return $http.get(url)
                .then(function (response) {
                    return response.data;
                });
        }

        function update(config) {
            return $http.put(url, config)
                .then(function (response) {
                    return response.data;
                });
        }
    }
})()