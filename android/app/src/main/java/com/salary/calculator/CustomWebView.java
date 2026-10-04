package com.salary.calculator;

import android.content.Context;
import android.util.AttributeSet;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputConnection;
import com.getcapacitor.CapacitorWebView;

/**
 * CustomWebView prevents Android from entering the fullscreen Extract UI in landscape mode.
 * When typing in landscape, the virtual keyboard stays docked at the bottom or floats,
 * keeping the app fully visible behind it.
 */
public class CustomWebView extends CapacitorWebView {

    public CustomWebView(Context context, AttributeSet attrs) {
        super(context, attrs);
    }

    @Override
    public InputConnection onCreateInputConnection(EditorInfo outAttrs) {
        InputConnection connection = super.onCreateInputConnection(outAttrs);
        if (outAttrs != null) {
            outAttrs.imeOptions |= EditorInfo.IME_FLAG_NO_EXTRACT_UI | EditorInfo.IME_FLAG_NO_FULLSCREEN;
        }
        return connection;
    }
}
