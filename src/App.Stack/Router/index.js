'use strict';

exports.handler = async (event, _, callback) => {
  const request = event.Records[0].cf.request;
  const MATCHING_EXTENSIONS = ['.js', '.css', '.json', '.txt', '.html', '.map', '.png', '.jpg', '.svg'];

  if (!MATCHING_EXTENSIONS.some(path => request.uri.endsWith(path))) {
    let prefix = '';

    if (request.uri.startsWith('/login')) {
      prefix = '/login';
    } else if (request.uri.startsWith('/account')) {
      prefix = '/account';
    }

    request.uri = `${prefix}/index.html`;
  }

  callback(null, request);
};
