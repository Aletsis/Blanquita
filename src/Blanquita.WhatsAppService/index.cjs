// Entry point wrapper for iisnode to bridge CommonJS (iisnode interceptor) to ESM
async function start() {
  await import('./dist/index.js');
}
start().catch(err => {
  console.error('Error starting WhatsApp Service via ESM import:', err);
  process.exit(1);
});
