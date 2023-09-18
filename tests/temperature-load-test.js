import http from 'k6/http';
import {sleep} from 'k6';

const baseUrl = 'https://localhost:5002/temperature';

export const options = {
  // Key configurations for avg load test in this section
  stages: [
    { duration: '5m', target: 100 }, // traffic ramp-up from 1 to 100 users over 5 minutes.
    { duration: '30m', target: 100 }, // stay at 100 users for 30 minutes
    { duration: '5m', target: 0 }, // ramp-down to 0 users
  ],
};

export default () => {
  const options = {
    headers: {
      'x-api-key': 'DD725D0D9DBB43479B760A0F1C95D43D',
      'User-Agent': 'k6-load-test',
    }
  };

  var locations =  ['Lahti', 'Nastola', 'Helsinki'];

  //console.log(requests);
  http.get(`https://localhost:5002/temperature?q=${locations[Math.floor(Math.random() * locations.length)]}`, options);
  sleep(1);
};