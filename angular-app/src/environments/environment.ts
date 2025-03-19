// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  //stripePublishableKey: 'pk_test_51N81XiI81Rrb3iDAhL1R2W2JVw1oAQK4NxxKqxEu0IXdxEVK5rm7XtSk0nwWrT4nJ7Rco9KHS0Gy3d05OhKnllfT00Q6Tib7Nx' // DevDB
  //stripePublishableKey: 'pk_live_51Kvzd2Ki9NY5HneShu1bYEZrFMavCdHjsG3SYebtWVzOZ9EnKkf3jUpu9oWFq35NzuNkZGxk8b0Ec4bW63BEZrqY00DrH3X3jY' // townend.dev - Live
  stripePublishableKey: 'pk_test_51Kvzd2Ki9NY5HneSjyGE2VDHiOt12r5wvR2YICTMNX5qiKfcRu7qVkMEhiZn1aIlbH26EO4jo6DZk2T6PNxnH92a00KOhBR1Go' // townend.dev - Test
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
