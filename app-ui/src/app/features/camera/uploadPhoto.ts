import { BatchResponse, SasUri } from '../../services/image/image.types';
import { UploadEventType, UploadMessage } from './camera.types';

export interface SasTokenHandler {
  getToken: (authToken: string, eventId: number) => Promise<SasUri>;
}

interface UploadPhotoProps {
  filenames: string[];
  fileCallback: (filename: string) => Promise<File>;
  eventId: number;
  galleryId?: number;
  authToken: string;
  apiBaseUrl: string;
  captureDate: Date;
  sasTokenHandler: SasTokenHandler;
  messageCallback?: (message: UploadMessage) => void;
  cleanup?: () => void;
}

export const uploadPhoto = async ({
  filenames,
  fileCallback,
  eventId,
  galleryId,
  authToken,
  apiBaseUrl,
  captureDate,
  sasTokenHandler,
  messageCallback,
  cleanup,
}: UploadPhotoProps) => {
  for (const filename of filenames) {
    if (messageCallback)
      messageCallback({
        eventType: UploadEventType.UploadStart,
        filename,
      });

    const file = await fileCallback(filename);
    const sas = await sasTokenHandler.getToken(authToken, eventId);
    const sasUploadUri = `${sas.baseUri}/${filename}?${sas.params}`;

    const uploadResponse = await fetch(sasUploadUri, {
      method: 'PUT',
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type || 'application/octet-stream',
      },
      body: file,
    });

    if (!uploadResponse.ok) {
      if (messageCallback)
        messageCallback({
          eventType: UploadEventType.UploadError,
          filename,
          error: uploadResponse.status,
        });
      if (cleanup) cleanup();
      return;
    }
  }

  const uploadMessage = JSON.stringify({
    photoFilenames: filenames,
    captureDate,
    eventId,
    galleryId,
  });

  const batchResponse = await fetch(`${apiBaseUrl}/image/upload`, {
    method: 'POST',
    body: uploadMessage,
    headers: {
      Authorization: `Bearer ${authToken}`,
      'Content-Type': 'application/json',
    },
  });

  if (!batchResponse.ok) {
    if (messageCallback)
      messageCallback({
        eventType: UploadEventType.UploadError,
        filename: '',
        error: batchResponse.status,
      });
    if (cleanup) cleanup();
    return;
  }
  const batch: BatchResponse = await batchResponse.json();

  if (messageCallback) {
    filenames.forEach((filename) => {
      messageCallback({
        eventType: UploadEventType.UploadComplete,
        filename,
      });
    });
  }

  if (cleanup) cleanup();
  return batch;
};
