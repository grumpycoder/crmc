//core.module.js
//mark.lawrence

(function () {
    angular.module('app.core',
        [
            //angular modules
            'ngMessages',
            'angularLocalStorage',
            'ui.router',
            'ngAnimate',

            //custom modules
            'blocks.logger',
            'blocks.exception',

            //third party modules
            'smart-table',
            'ui.bootstrap',
            'ngTagsInput',
            'ngFileUpload',
            'rzModule',
            'switcher',
            'gfl.textAvatar'
        ])
        .constant('toastr', toastr)
        .constant('moment', moment);;
})();