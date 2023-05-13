package com.example.deliverymanapp

import com.android.volley.NetworkResponse
import com.android.volley.ParseError
import com.android.volley.Response
import com.android.volley.toolbox.HttpHeaderParser
import com.android.volley.toolbox.JsonObjectRequest
import org.json.JSONException
import org.json.JSONObject
import java.io.UnsupportedEncodingException


class MetaRequest : JsonObjectRequest {
    constructor(
        method: Int,
        url: String?,
        jsonRequest: JSONObject?,
        listener: Response.Listener<JSONObject?>?,
        errorListener: Response.ErrorListener?
    ) : super(method, url, jsonRequest, listener, errorListener) {
    }

    constructor(
        url: String?,
        jsonRequest: JSONObject?,
        listener: Response.Listener<JSONObject?>?,
        errorListener: Response.ErrorListener?
    ) : super(url, jsonRequest, listener, errorListener) {
    }

    override fun parseNetworkResponse(response: NetworkResponse): Response<JSONObject> {
        return try {
            val jsonString = String(response.data)
            val jsonResponse = JSONObject()

            response.allHeaders?.forEach {
                var nameEl:String = it.name
                if(jsonResponse.has(it.name))
                    nameEl += "1"
                jsonResponse.put(nameEl, it.value)
            }
            if(jsonString != "")
                jsonResponse.put("body", jsonString)

            Response.success(
                jsonResponse,
                HttpHeaderParser.parseCacheHeaders(response)
            )
        } catch (e: UnsupportedEncodingException) {
            Response.error(ParseError(e))
        } catch (je: JSONException) {
            Response.error(ParseError(je))
        }
    }
}