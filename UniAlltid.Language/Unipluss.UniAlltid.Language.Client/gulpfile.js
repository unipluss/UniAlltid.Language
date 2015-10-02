var gulp = require('gulp');

// Include plugins
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var rename = require('gulp-rename');
// Concatenate JS Files
gulp.task('scripts', function () {
    return gulp.src(
            ['Scripts/jquery-1.9.1.min.js',
            'Scripts/bootstrap.min.js',
            'Scripts/angular.min.js',
            'Scripts/angular-resource.min.js',
            'Scripts/xeditable.min.js',
            'Scripts/angular-ui-notification.min.js',
            'Scripts/dirPagination.js',
            'app/app.js',
            'common/common.services.js',
            'common/language.service.js',
            'app/language/languages.controller.js'])
      .pipe(concat('main.js'))
        .pipe(rename({ suffix: '.min' }))
        .pipe(uglify())
        .pipe(gulp.dest('deploy/js'));
});
// Default Task
gulp.task('default', ['scripts']);