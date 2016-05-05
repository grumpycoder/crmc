//config.js
//mark.lawrence

(function () {
    var core = angular.module('app.core');

    var keyCodes = {
        backspace: 8,
        tab: 9,
        enter: 13,
        esc: 27,
        space: 32,
        pageup: 33,
        pagedown: 34,
        end: 35,
        home: 36,
        left: 37,
        up: 38,
        right: 39,
        down: 40,
        insert: 45,
        del: 46
    };

    var apiEndPoints = {
        Censor: 'censor',
        Person: 'person',
        User: 'users',
        Configuration: 'configuration'
    }

    var config = {
        appErrorPrefix: '[CRMC Error] ', //Configure the exceptionHandler decorator
        appTitle: 'CRMC',
        version: '1.0.0',
        apiUrl: 'http://localhost:11277/api/',
        keyCodes: keyCodes,
        apiEndPoints: apiEndPoints
    };

    var defaults = {
        EMAIL_SUFFIX: '@splcenter.org',
        GENERIC_PASSWORD: '1P@ssword'
    }

    core.constant('config', config);
    core.value('defaults', defaults);
})();