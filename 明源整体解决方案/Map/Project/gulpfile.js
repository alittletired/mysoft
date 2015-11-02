var gulp = require('gulp');
var fs = require('fs');
var path = require('path');
//var buffer = require('buffer');
//var foreach = require('gulp-foreach');
var debug = require('gulp-debug');
var through = require('through2');
var minify= require('gulp-minify');
//var rename = require("gulp-rename");
var concat = require('gulp-concat');
var gutil = require('gulp-util');

function addVersion() {

  var stream = through.obj(function(file, enc, cb) {



    if (file.path.indexOf('seaConfig.json') > 0) {
      var jsonStr = file.contents.toString('utf8');
      var seaConfig = JSON.parse(jsonStr);
      for (var alia in seaConfig.alias) {
        //  console.log(alia);
        var filepath = seaConfig.alias[alia];
        filepath = filepath.split('?')[0];
        if (filepath.substring(filepath.length - 3) !== ".js") {
          filepath += '.js'
        }

        var stats = fs.statSync(path.resolve('js', filepath));
        var mtime = parseInt(stats.mtime.getTime() / 1000);
        filepath += '?v=' + mtime;
        console.log(filepath);
        seaConfig.alias[alia] = filepath;

      }
      var str = JSON.stringify(seaConfig, null, 4);
      str = 'var config=' + str + ';\nseajs.config(config);'
      file.contents = new Buffer(str)
    }
    this.push(file);

    cb();
  });

  return stream;
}

gulp.task('seajs', function() {
  gulp.src(['./js/sea.min.js', './js/seaConfig.json'])
    .pipe(addVersion())
  .pipe(debug({
      title: 'seajs'
    }))
    .pipe(concat('sea.js'))
    .pipe(gulp.dest('./js'));
});

gulp.task('default', function() {
  gulp.start('seajs');
});
