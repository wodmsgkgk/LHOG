package com.example.android.BluetoothChat;

import android.hardware.Camera;
import android.hardware.Camera.PreviewCallback;
import android.os.Bundle;
import android.app.Activity;
import android.content.pm.ActivityInfo;
import android.view.Menu;
import android.view.SurfaceHolder;
import android.widget.VideoView;



public class MainActivity extends Activity implements SurfaceHolder.Callback{
	

	Camera mCamera ;
	VideoView mVideoView;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.main);
		
		setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE);
		mVideoView = (VideoView)findViewById(R.id.vidoeView);
		final SurfaceHolder holder = mVideoView.getHolder();
		holder.addCallback(this);
		
		holder.setType(SurfaceHolder.SURFACE_TYPE_PUSH_BUFFERS);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		//getMenuInflater().inflate(R.menu.option_menu);
		return true;
	}

	@Override
	public void surfaceChanged(SurfaceHolder holder, int format, int width,
			int height) {
		// TODO Auto-generated method stub
		Camera.Parameters parameters = mCamera.getParameters();
		
		parameters.setPreviewSize(width, height);
		
		mCamera.setParameters(parameters);
		mCamera.startPreview();
	}

	@Override
	public void surfaceCreated(SurfaceHolder holder) {
		// TODO Auto-generated method stub
		try{
			mCamera = Camera.open();
			Camera.Parameters parameters = mCamera.getParameters();
			parameters.setRotation(90);
			mCamera.setParameters(parameters);			
			mCamera.setPreviewDisplay(holder);
			mCamera.setPreviewCallback(new PreviewCallback() {
				
				@Override
				public void onPreviewFrame(byte[] data, Camera camera) {
					// TODO Auto-generated method stub
					
				}
			});
			
		}
		catch(Exception e){
			
		}
	}

	@Override
	public void surfaceDestroyed(SurfaceHolder holder) {
		// TODO Auto-generated method stub
		mCamera.stopPreview();
		mCamera =null;	
		
	}


}
