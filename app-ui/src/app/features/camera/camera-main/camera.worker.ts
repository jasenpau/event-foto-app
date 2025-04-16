/// <reference lib="webworker" />

import { UploadEventType, UploadMessage } from '../camera.types';

interface SasUriResponse {
  sasUri: string;
  expiresOn: string;
  eventId: number;
}

const sasTokenData = {
  sasUri: '',
  expiresOn: new Date(0),
  eventId: 0,
};

addEventListener('message', async ({ data }) => {
  const { filename, eventId, captureDate, authToken, apiBaseUrl } = data;

  if (!filename || !eventId) {
    postMessage({
      eventType: UploadEventType.UploadError,
      error: 'Invalid filename or event id.',
    });
    return;
  }

  postMessage({
    eventType: UploadEventType.UploadStart,
    filename,
  });

  try {
    const root = await navigator.storage.getDirectory();
    const fileHandle = await root.getFileHandle(filename);
    const file = await fileHandle.getFile();

    const sasContainerUri = await acquireSasUri(eventId, authToken, apiBaseUrl);
    const sasParts = sasContainerUri.split('?');
    const sasUri = `${sasParts[0]}/${filename}?${sasParts[1]}`;

    const uploadResponse = await fetch(sasUri, {
      method: 'PUT',
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type || 'application/octet-stream',
      },
      body: file,
    });
    console.log('[worker]', uploadResponse);
    if (!uploadResponse.ok) {
      postMessage({
        eventType: UploadEventType.UploadError,
        filename,
        error: uploadResponse.status,
      });
      return;
    }

    const uploadMessage = JSON.stringify({
      filename,
      captureDate,
      eventId,
    });

    const uploadMessageResponse = await fetch(`${apiBaseUrl}/image/upload`, {
      method: 'POST',
      body: uploadMessage,
      headers: {
        Authorization: `Bearer ${authToken}`,
        'Content-Type': 'application/json',
      },
    });

    if (!uploadMessageResponse.ok) {
      postMessage({
        eventType: UploadEventType.UploadError,
        filename,
        error: uploadMessageResponse.status,
      });
    }

    await root.removeEntry(filename);
    postMessage({
      eventType: UploadEventType.UploadComplete,
      filename,
    });
  } catch (error) {
    sendMessage({
      eventType: UploadEventType.UploadError,
      filename,
      error: error,
    });
  }
});

const acquireSasUri = async (
  eventId: number,
  authToken: string,
  apiBaseUrl: string,
) => {
  if (
    sasTokenData.sasUri &&
    sasTokenData.expiresOn > new Date() &&
    eventId === sasTokenData.eventId
  ) {
    return sasTokenData.sasUri;
  }

  const response = await fetch(`${apiBaseUrl}/image/sas/${eventId}`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${authToken}`,
    },
  });

  if (response.ok) {
    const sasResponse = (await response.json()) as SasUriResponse;
    sasTokenData.sasUri = sasResponse.sasUri;
    sasTokenData.expiresOn = new Date(sasResponse.expiresOn);
    return sasTokenData.sasUri;
  }

  throw Error('Cannot acquire SAS URI');
};

const sendMessage = (message: UploadMessage) => {
  postMessage(message);
};
