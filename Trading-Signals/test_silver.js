// const fs = require('fs');

async function test() {
  const response = await fetch('https://giabac.phuquygroup.vn/PhuQuyPrice/SilverPricePartial', {
    headers: { 'Accept': 'text/html, */*; q=0.01', 'X-Requested-With': 'XMLHttpRequest' }
  });
  const text = await response.text();
  console.log(text);
}
test();
