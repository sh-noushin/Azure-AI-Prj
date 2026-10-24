/**
 * The browser records in WebM, but the Speech service wants WAV.
 * These helpers decode the recording and write it out as 16 kHz mono WAV.
 */

const SAMPLE_RATE = 16000;

/** Converts a browser recording into a WAV file the Speech service accepts. */
export async function toWav(recording: Blob): Promise<Blob> {
  const bytes = await recording.arrayBuffer();

  const context = new AudioContext();
  const decoded = await context.decodeAudioData(bytes);
  await context.close();

  // Re-render at 16 kHz in mono, which is what speech recognition expects.
  const offline = new OfflineAudioContext(1, Math.ceil(decoded.duration * SAMPLE_RATE), SAMPLE_RATE);
  const source = offline.createBufferSource();
  source.buffer = decoded;
  source.connect(offline.destination);
  source.start();

  const rendered = await offline.startRendering();
  return encodeWav(rendered.getChannelData(0));
}

/** Writes the samples out with a WAV header. */
function encodeWav(samples: Float32Array): Blob {
  const buffer = new ArrayBuffer(44 + samples.length * 2);
  const view = new DataView(buffer);

  writeText(view, 0, 'RIFF');
  view.setUint32(4, 36 + samples.length * 2, true);
  writeText(view, 8, 'WAVE');

  writeText(view, 12, 'fmt ');
  view.setUint32(16, 16, true); // header size
  view.setUint16(20, 1, true); // 1 = uncompressed PCM
  view.setUint16(22, 1, true); // mono
  view.setUint32(24, SAMPLE_RATE, true);
  view.setUint32(28, SAMPLE_RATE * 2, true); // bytes per second
  view.setUint16(32, 2, true); // bytes per sample
  view.setUint16(34, 16, true); // bits per sample

  writeText(view, 36, 'data');
  view.setUint32(40, samples.length * 2, true);

  // Float values from -1..1 become whole numbers from -32768..32767.
  let offset = 44;
  for (const sample of samples) {
    const clamped = Math.max(-1, Math.min(1, sample));
    view.setInt16(offset, clamped * 0x7fff, true);
    offset += 2;
  }

  return new Blob([view], { type: 'audio/wav' });
}

function writeText(view: DataView, offset: number, text: string): void {
  for (let i = 0; i < text.length; i++) {
    view.setUint8(offset + i, text.charCodeAt(i));
  }
}
